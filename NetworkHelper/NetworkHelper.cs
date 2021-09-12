// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using Windows.Devices.WiFi;
using System.Net.NetworkInformation;

namespace nanoFramework.Networking
{
    /// <summary>
    /// Network helper class to connect easily to WiFi networks
    /// </summary>
    public static class NetworkHelper
    {
        private static string _ssid;
        private static string _password;
        private static WiFiAdapter _wifi;
        private static WiFiReconnectionKind _reconnectionKind;
        private static IPConfiguration _ipConfiguration;

        /// <summary>
        /// The error connection type
        /// </summary>
        public static ConnectionError ConnectionError { get; internal set; } = new ConnectionError("Not setup yet");

        /// <summary>
        /// This function will connect the network with DHCP enabled, for your SSID and try to connect to it with the credentials you've passed. This will save as well
        /// the configuration of your network.
        /// </summary>
        /// <param name="ssid">The SSID you are trying to connect to.</param>
        /// <param name="password">The password associated to the SSID you are trying to connect to.</param>
        /// <param name="reconnectionKind">The reconnection type to the network.</param>
        /// <param name="setDateTime">True to wait for a valid date time.</param>
        /// <param name="wifiAdapter">The WiFi adapter to be used. Relevant only if you have multiple WiFi adapters.</param>
        /// <param name="token">A cancellation token used for timeout.</param>
        /// <returns>True if success. If no success a more detailed error message available in the ConnectionError property</returns>
        public static bool ConnectWifiDhcp(string ssid, string password, WiFiReconnectionKind reconnectionKind = WiFiReconnectionKind.Automatic, bool setDateTime = false, int wifiAdapter = 0, CancellationToken token = default(CancellationToken))
            => ScanAndConnectWifi(ssid, password, null, false, reconnectionKind, setDateTime, wifiAdapter, token);

        /// <summary>
        /// This function will connect the network with the static IP address you are providing, for your SSID and try to connect to it with the credentials you've passed. This will save as well
        /// the configuration of your network.
        /// </summary>
        /// <param name="ssid">The SSID you are trying to connect to.</param>
        /// <param name="password">The password associated to the SSID you are trying to connect to.</param>
        /// <param name="ipConfiguration">The static IP configuration you want to apply.</param>
        /// <param name="reconnectionKind">The reconnection type to the network.</param>
        /// <param name="setDateTime">True to wait for a valid date time.</param>
        /// <param name="wifiAdapter">The WiFi adapter to be used. Relevant only if you have multiple WiFi adapters.</param>
        /// <param name="token">A cancellation token used for timeout.</param>
        /// <returns>True if success. If no success a more detailed error message available in the ConnectionError property</returns>
        public static bool ConnectWifiFixAddress(string ssid, string password, IPConfiguration ipConfiguration, WiFiReconnectionKind reconnectionKind = WiFiReconnectionKind.Automatic, bool setDateTime = false, int wifiAdapter = 0, CancellationToken token = default(CancellationToken))
            => ScanAndConnectWifi(ssid, password, ipConfiguration, false, reconnectionKind, setDateTime, wifiAdapter, token);

        /// <summary>
        /// This function will scan and connect the network with DHCP enabled, for your SSID and try to connect to it with the credentials you've passed. This will save as well
        /// the configuration of your network. 
        /// </summary>
        /// <param name="ssid">The SSID you are trying to connect to.</param>
        /// <param name="password">The password associated to the SSID you are trying to connect to.</param>
        /// <param name="reconnectionKind">The reconnection type to the network.</param>
        /// <param name="setDateTime">True to wait for a valid date time.</param>
        /// <param name="wifiAdapter">The WiFi adapter to be used. Relevant only if you have multiple WiFi adapters.</param>
        /// <param name="token">A cancellation token used for timeout.</param>
        /// <returns>True if success. If no success a more detailed error message available in the ConnectionError property</returns>
        public static bool ScanAndConnectWifiDhcp(string ssid, string password, WiFiReconnectionKind reconnectionKind = WiFiReconnectionKind.Automatic, bool setDateTime = false, int wifiAdapter = 0, CancellationToken token = default(CancellationToken))
            => ScanAndConnectWifi(ssid, password, null, true, reconnectionKind, setDateTime, wifiAdapter, token);

        /// <summary>
        /// This function will connect the network, the information used to connect are the ones stored in the device.
        /// This function can be called only if an existing network has been setup previously and if the credentials are valid.
        /// </summary>
        /// <param name="setDateTime">True to wait for a valid date time.</param>
        /// <param name="wifiAdapter">The WiFi adapter to be used. Relevant only if you have multiple WiFi adapters.</param>
        /// <param name="token">A cancellation token used for timeout.</param>
        /// <returns>True if success. If no success a more detailed error message available in the ConnectionError property</returns>
        public static bool ReconnectWifi(bool setDateTime = false, int wifiAdapter = 0, CancellationToken token = default(CancellationToken))
        {
            try
            {
                _wifi = WiFiAdapter.FindAllAdapters()[wifiAdapter];
                NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface n in nis)
                {
                    if (n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        _wifi.ScanAsync();
                        break;
                    }
                }

                return WaitForValidIPAndDate(setDateTime, NetworkInterfaceType.Wireless80211, token);
            }
            catch (Exception ex)
            {
                ConnectionError = new ConnectionError("Exception", ex);
                return false;
            }

        }

        /// <summary>
        /// Checks if a WiFi configuration is stored.
        /// </summary>
        /// <param name="wifiAdapter">The WiFi adapter to be used. Relevant only if you have multiple WiFi adapters.</param>
        /// <returns>True if a configuration is stored.</returns>
        public static bool IsConfigurationStored(int wifiAdapter = 0)
        {
            _wifi = WiFiAdapter.FindAllAdapters()[wifiAdapter];
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface n in nis)
            {
                if (n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    Wireless80211Configuration wc = Wireless80211Configuration.GetAllWireless80211Configurations()[n.SpecificConfigId];
                    return !string.IsNullOrEmpty(wc.Ssid);
                }
            }

            return false;
        }

        private static bool ScanAndConnectWifi(string ssid, string password, IPConfiguration ipConfiguration, bool scan, WiFiReconnectionKind reconnectionKind, bool setDateTime, int wifiAdapter, CancellationToken token)
        {
            try
            {
                _ssid = ssid;
                _password = password;
                _wifi = WiFiAdapter.FindAllAdapters()[wifiAdapter];
                _reconnectionKind = reconnectionKind;
                _ipConfiguration = ipConfiguration;
                bool isAlreadyConnected = false;
                // Check if we are already connected
                if (IsValidIpAddress(NetworkInterfaceType.Wireless80211))
                {
                    // Check if it's already the correct network
                    NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface ni in nis)
                    {
                        if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                        {
                            Wireless80211Configuration wc = Wireless80211Configuration.GetAllWireless80211Configurations()[ni.SpecificConfigId];
                            // Let's make sure this configuration is saved
                            if (wc.Ssid == _ssid)
                            {
                                isAlreadyConnected = true;
                                break;
                            }
                        }
                    }

                    if (!isAlreadyConnected)
                    {
                        _wifi.Disconnect();
                        isAlreadyConnected = false;
                    }
                }

                if (!isAlreadyConnected)
                {
                    // Set up the AvailableNetworksChanged event to pick up when scan has completed
                    if (scan)
                    {
                        _wifi.AvailableNetworksChanged += WifiAvailableNetworksChanged;
                        Debug.WriteLine("starting WiFi scan");
                        _wifi.ScanAsync();
                    }
                    else
                    {
                        NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
                        foreach (NetworkInterface ni in nis)
                        {
                            if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                            {
                                _wifi.Disconnect();
                                // Make sure we store the configuration
                                StoreConfiguration(ni);
                                if (_ipConfiguration != null)
                                {
                                    ni.EnableStaticIPv4(_ipConfiguration.IPAddress, _ipConfiguration.IPSubnetMask, _ipConfiguration.IPGatewayAddress);
                                    if ((_ipConfiguration.IPDns != null) && (_ipConfiguration.IPDns.Length > 0))
                                    {
                                        ni.EnableStaticIPv4Dns(_ipConfiguration.IPDns);
                                    }
                                }
                                else
                                {
                                    ni.EnableDhcp();
                                    ni.EnableAutomaticDns();
                                }

                                _wifi.Connect(_ssid, _reconnectionKind, _password);
                                break;
                            }
                        }
                    }
                }

                return WaitForValidIPAndDate(setDateTime, NetworkInterfaceType.Wireless80211, token);
            }
            catch (Exception ex)
            {
                ConnectionError = new ConnectionError("Exception", ex);
                return false;
            }

        }

        /// <summary>
        /// Wait for a valid IP and potentially a date
        /// </summary>
        /// <param name="setDateTime">True to wait for a valid date</param>
        /// <param name="networkInterfaceType">The tye of interface.</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>True if success</returns>
        public static bool WaitForValidIPAndDate(bool setDateTime, NetworkInterfaceType networkInterfaceType, CancellationToken token)
        {
            Debug.WriteLine("Checking for IP");
            while (!token.IsCancellationRequested && !IsValidIpAddress(networkInterfaceType))
            {
                Thread.Sleep(200);
            }

            if (token.IsCancellationRequested)
            {
                ConnectionError = new ConnectionError("Token expired while checking IP address", null);
                return false;
            }

            if (setDateTime)
            {
                Debug.WriteLine("Setting up system clock...");
                while (!token.IsCancellationRequested && !IsValidDateTime())
                {
                    Thread.Sleep(200);
                }
            }

            if (token.IsCancellationRequested)
            {
                ConnectionError = new ConnectionError("Token expired while setting system clock", null);
                return false;
            }

            if (setDateTime)
            {
                Debug.WriteLine($"We do have a valid date {DateTime.UtcNow}");
            }

            ConnectionError = new ConnectionError("No error", null);
            return true;
        }

        /// <summary>
        /// Check if the DateTime is valid.
        /// </summary>
        /// <returns>True if valid.</returns>
        public static bool IsValidDateTime() => DateTime.UtcNow.Year > 2018;

        /// <summary>
        /// Check if there is a valid IP address on a specific interface type.
        /// </summary>
        /// <param name="interfaceType">The type of interface.</param>
        /// <returns>True if valid.</returns>
        public static bool IsValidIpAddress(NetworkInterfaceType interfaceType)
        {
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in nis)
            {
                if (ni.NetworkInterfaceType != interfaceType)
                {
                    break;
                }

                if (ni.IPv4Address != null && ni.IPv4Address.Length > 0)
                {
                    if (ni.IPv4Address[0] != '0')
                    {
                        Debug.WriteLine($"We have and IP: {ni.IPv4Address}");
                        return true;
                    }
                }
            }

            return false;
        }

        private static void StoreConfiguration(NetworkInterface ni)
        {
            Wireless80211Configuration wc = Wireless80211Configuration.GetAllWireless80211Configurations()[ni.SpecificConfigId];
            // Let's make sure this configuration is saved
            wc.Ssid = _ssid;
            wc.Password = _password;
            wc.SaveConfiguration();
        }

        private static void WifiAvailableNetworksChanged(WiFiAdapter sender, object e)
        {
            Debug.WriteLine("Wifi_AvailableNetworksChanged - get report");

            // Get Report of all scanned WiFi networks
            WiFiNetworkReport report = sender.NetworkReport;

            // Enumerate though networks looking for our network
            foreach (WiFiAvailableNetwork net in report.AvailableNetworks)
            {
                // Show all networks found
                Debug.WriteLine($"Net SSID :{net.Ssid},  BSSID : {net.Bsid},  rssi : {net.NetworkRssiInDecibelMilliwatts.ToString()},  signal : {net.SignalBars.ToString()}");

                // If its our Network then try to connect
                if (net.Ssid == _ssid)
                {
                    // Disconnect in case we are already connected
                    sender.Disconnect();

                    // get the first interface
                    NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
                    NetworkInterface ni = null;
                    foreach (NetworkInterface n in nis)
                    {
                        if (n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                        {
                            ni = n;
                            break;
                        }
                    }

                    ni.EnableAutomaticDns();
                    if (_ipConfiguration == null)
                    {
                        ni.EnableDhcp();
                    }
                    else
                    {
                        ni.EnableStaticIPv4(_ipConfiguration.IPAddress, _ipConfiguration.IPSubnetMask, _ipConfiguration.IPGatewayAddress);
                    }

                    // Connect to network
                    WiFiConnectionResult result = sender.Connect(net, _reconnectionKind, _password);

                    // Display status
                    if (result.ConnectionStatus == WiFiConnectionStatus.Success)
                    {
                        sender.AvailableNetworksChanged -= WifiAvailableNetworksChanged;
                        StoreConfiguration(ni);
                        return;
                    }
                    else
                    {
                        ConnectionError = new ConnectionError("Error connecting to WiFi wile scanning");
                    }
                }
            }
        }
    }
}
