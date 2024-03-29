﻿//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Device.Wifi;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace nanoFramework.Networking
{
    /// <summary>
    /// Network helper class providing helper methods to assist on connecting to a Wifi network.
    /// </summary>
    public static class WifiNetworkHelper
    {
        private static ManualResetEvent _ipAddressAvailable;
        private static ManualResetEvent _networkReady = new(false);
        private static bool _requiresDateTime;

        private static string _ssid;
        private static string _password;
        private static WifiAdapter _wifi = null;
        private static WifiReconnectionKind _reconnectionKind;
        private static IPConfiguration _ipConfiguration;

        private static NetworkHelperStatus _networkHelperStatus = NetworkHelperStatus.None;
        private static Exception _helperException;

        // defaulting to Ethernet
        private static NetworkInterfaceType _workingNetworkInterface = NetworkInterfaceType.Wireless80211;

        /// <summary>
        /// This flag will make sure there is only one and only call to any of the helper methods.
        /// </summary>
        private static bool _helperInstanciated = false;

        /// <summary>
        /// Event signaling that networking it's ready.
        /// </summary>
        /// <remarks>
        /// The conditions for this are setup in the call to <see cref="WifiNetworkHelper.SetupNetworkHelper"/>. 
        /// It will be a composition of network connected, IpAddress available and valid system <see cref="DateTime"/>.</remarks>
        public static ManualResetEvent NetworkReady => _networkReady;

        /// <summary>
        /// Status of NetworkHelper.
        /// </summary>
        public static NetworkHelperStatus Status => _networkHelperStatus;

        /// <summary>
        /// Exception that occurred when waiting for the network to become ready.
        /// </summary>
        public static Exception HelperException { get => _helperException; internal set => _helperException = value; }

        /// <summary>
        /// This method will setup the network configurations so that the <see cref="NetworkReady"/> event will fire when the required conditions are met.
        /// That will be the network connection to be up, having a valid IpAddress and optionally for a valid date and time to become available.
        /// </summary>
        /// <param name="requiresDateTime">Set to <see langword="true"/> if valid date and time are required.</param>
        /// <exception cref="InvalidOperationException">If any of the <see cref="NetworkHelper"/> methods is called more than once.</exception>
        /// <exception cref="NotSupportedException">There is no network interface configured. Open the 'Edit Network Configuration' in Device Explorer and configure one.</exception>
        public static void SetupNetworkHelper(bool requiresDateTime = false)
        {
            _requiresDateTime = requiresDateTime;

            SetupHelper(true);

            // fire working thread
            new Thread(WorkingThread).Start();
        }

        /// <summary>
        /// This method will setup the network configurations so that the <see cref="NetworkReady"/> event will fire when the required conditions are met.
        /// That will be the network connection to be up, having a valid IpAddress and optionally for a valid date and time to become available.
        /// </summary>
        /// <param name="ipConfiguration">The static IP configuration you want to apply.</param>
        /// <param name="requiresDateTime">Set to <see langword="true"/> if valid date and time are required.</param>
        /// <exception cref="NotSupportedException">There is no network interface configured. Open the 'Edit Network Configuration' in Device Explorer and configure one.</exception>
        public static void SetupNetworkHelper(
            IPConfiguration ipConfiguration,
            bool requiresDateTime = false)
        {
            _requiresDateTime = requiresDateTime;
            _ipConfiguration = ipConfiguration;

            SetupHelper(true);

            // fire working thread
            new Thread(WorkingThread).Start();
        }

        /// <summary>
        /// This method will setup the network configurations so that the <see cref="NetworkReady"/> event will fire when the required conditions are met.
        /// That will be the network connection to be up, having a valid IpAddress and optionally for a valid date and time to become available.
        /// </summary>
        /// <param name="requiresDateTime">Set to <see langword="true"/> if valid date and time are required.</param>
        /// <param name="ssid">The SSID of the network you are trying to connect to.</param>
        /// <param name="password">The password associated to the SSID of the network you are trying to connect to.</param>
        /// <param name="reconnectionKind">The <see cref="WifiReconnectionKind"/> to setup for the connection.</param>
        /// <exception cref="InvalidOperationException">If any of the <see cref="NetworkHelper"/> methods is called more than once.</exception>
        /// <exception cref="NotSupportedException">There is no network interface configured. Open the 'Edit Network Configuration' in Device Explorer and configure one.</exception>
        public static void SetupNetworkHelper(
            string ssid,
            string password,
            WifiReconnectionKind reconnectionKind = WifiReconnectionKind.Automatic,
            bool requiresDateTime = false)
        {
            _requiresDateTime = requiresDateTime;
            _ssid = ssid;
            _password = password;
            _reconnectionKind = reconnectionKind;

            SetupHelper(true);

            // fire working thread
            new Thread(WorkingThread).Start();
        }

        /// <summary>
        /// Disconnect from the network.
        /// </summary>
        public static void Disconnect()
        {
            _wifi?.Disconnect();
            _wifi?.Dispose();
            _wifi = null;
        }

        /// <summary>
        /// Gets the Wifi Adapter.
        /// </summary>
        public static WifiAdapter WifiAdapter { get => _wifi; }

        /// <summary>
        /// This method will connect the network with DHCP enabled, for your SSID and try to connect to it with the credentials you've passed. This will save as well
        /// the configuration of your network.
        /// </summary>
        /// <param name="ssid">The SSID of the network you are trying to connect to.</param>
        /// <param name="password">The password associated to the SSID of the network you are trying to connect to.</param>
        /// <param name="reconnectionKind">The <see cref="WifiReconnectionKind"/> to setup for the connection.</param>
        /// <param name="requiresDateTime">Set to <see langword="true"/> if valid date and time are required.</param>
        /// <param name="wifiAdapterId">The index of the Wifi adapter to be used. Relevant only if there are multiple Wifi adapters.</param>
        /// <param name="token">A <see cref="CancellationToken"/> used for timing out the operation.</param>
        /// <returns><see langword="true"/> on success. On failure returns <see langword="false"/> and details with the cause of the failure are made available in the <see cref="Status"/> property</returns>
        public static bool ConnectDhcp(
            string ssid,
            string password,
            WifiReconnectionKind reconnectionKind = WifiReconnectionKind.Automatic,
            bool requiresDateTime = false,
            int wifiAdapterId = 0,
            CancellationToken token = default)
            => ScanAndConnect(
                ssid,
                password,
                null,
                false,
                reconnectionKind,
                requiresDateTime,
                wifiAdapterId,
                token);

        /// <summary>
        /// This method will connect the network with the static IP address you are providing, for your SSID and try to connect to it with the credentials you've passed. This will save as well
        /// the configuration of your network.
        /// </summary>
        /// <param name="ssid">The SSID you are trying to connect to.</param>
        /// <param name="password">The password associated to the SSID you are trying to connect to.</param>
        /// <param name="ipConfiguration">The static IPv4 configuration to apply to the Wifi network interface.</param>
        /// <param name="reconnectionKind">The <see cref="WifiReconnectionKind"/> to setup for the connection.</param>
        /// <param name="requiresDateTime">Set to <see langword="true"/> if valid date and time are required.</param>
        /// <param name="WifiAdapter">The index of the Wifi adapter to be used. Relevant only if there are multiple Wifi adapters.</param>
        /// <param name="token">A <see cref="CancellationToken"/> used for timing out the operation.</param>
        /// <returns><see langword="true"/> on success. On failure returns <see langword="false"/> and details with the cause of the failure are made available in the <see cref="Status"/> property</returns>
        public static bool ConnectFixAddress(
            string ssid,
            string password,
            IPConfiguration ipConfiguration,
            WifiReconnectionKind reconnectionKind = WifiReconnectionKind.Automatic,
            bool requiresDateTime = false,
            int WifiAdapter = 0,
            CancellationToken token = default)
            => ScanAndConnect(
                ssid,
                password,
                ipConfiguration,
                false,
                reconnectionKind,
                requiresDateTime,
                WifiAdapter,
                token);

        /// <summary>
        /// This method will scan and connect the network with DHCP enabled, for your SSID and try to connect to it with the credentials you've passed. This will save as well
        /// the configuration of your network. 
        /// </summary>
        /// <param name="ssid">The SSID you are trying to connect to.</param>
        /// <param name="password">The password associated to the SSID you are trying to connect to.</param>
        /// <param name="reconnectionKind">The reconnection type to the network.</param>
        /// <param name="requiresDateTime">Set to <see langword="true"/> if valid date and time are required.</param>
        /// <param name="WifiAdapter">The index of the Wifi adapter to be used. Relevant only if there are multiple Wifi adapters.</param>
        /// <param name="token">A <see cref="CancellationToken"/> used for timing out the operation.</param>
        /// <returns><see langword="true"/> on success. On failure returns <see langword="false"/> and details with the cause of the failure are made available in the <see cref="Status"/> property</returns>
        public static bool ScanAndConnectDhcp(
            string ssid,
            string password,
            WifiReconnectionKind reconnectionKind = WifiReconnectionKind.Automatic,
            bool requiresDateTime = false,
            int WifiAdapter = 0,
            CancellationToken token = default)
            => ScanAndConnect(
                ssid,
                password,
                null,
                true,
                reconnectionKind,
                requiresDateTime,
                WifiAdapter,
                token);

        /// <summary>
        /// This method will connect the network, the information used to connect is the one already stored on the device.
        /// </summary>
        /// <param name="requiresDateTime">Set to <see langword="true"/> if valid date and time are required.</param>
        /// <param name="wifiAdapterId">The index of the Wifi adapter to be used. Relevant only if there are multiple Wifi adapters.</param>
        /// <param name="token">A <see cref="CancellationToken"/> used for timing out the operation.</param>
        /// <returns><see langword="true"/> on success. On failure returns <see langword="false"/> and details with the cause of the failure are made available in the <see cref="Status"/> property</returns>
        /// <remarks>
        /// This function can be called only if an existing network has been setup previously and if the credentials are valid.
        /// </remarks>
        public static bool Reconnect(
            bool requiresDateTime = false,
            int wifiAdapterId = 0,
            CancellationToken token = default)
        {
            try
            {
                _wifi = WifiAdapter.FindAllAdapters()[wifiAdapterId];

                return NetworkHelper.InternalWaitNetworkAvailable(
                    _workingNetworkInterface,
                    ref _networkHelperStatus,
                    false,
                    token,
                    requiresDateTime);
            }
            catch (Exception ex)
            {
                _networkHelperStatus = NetworkHelperStatus.ExceptionOccurred;
                HelperException = ex;

                return false;
            }
        }

        private static bool ScanAndConnect(
            string ssid,
            string password,
            IPConfiguration ipConfiguration,
            bool scan,
            WifiReconnectionKind reconnectionKind,
            bool setDateTime,
            int wifiAdapterId,
            CancellationToken token)
        {
            try
            {
                _ssid = ssid;
                _password = password;
                _wifi = WifiAdapter.FindAllAdapters()[wifiAdapterId];
                _reconnectionKind = reconnectionKind;
                _ipConfiguration = ipConfiguration;
                bool isAlreadyConnected = false;

                // Check if we are already connected
                // need to 
                _workingNetworkInterface = NetworkInterfaceType.Wireless80211;

                if (NetworkHelperInternal.CheckIP(
                    _workingNetworkInterface,
                    _ipConfiguration))
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

                        Debug.WriteLine("starting Wifi scan");
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
                                StoreWifiConfiguration(ni);

                                if (_ipConfiguration != null)
                                {
                                    ni.EnableStaticIPv4(
                                        _ipConfiguration.IPAddress,
                                        _ipConfiguration.IPSubnetMask,
                                        _ipConfiguration.IPGatewayAddress);

                                    if (_ipConfiguration.IPDns != null
                                        && _ipConfiguration.IPDns.Length > 0)
                                    {
                                        ni.EnableStaticIPv4Dns(_ipConfiguration.IPDns);
                                    }
                                }
                                else
                                {
                                    if (ni.IsDhcpEnabled)
                                    {
                                        ni.EnableDhcp();
                                    }
                                    else
                                    {
                                        // setup static IPv4 with network config
                                        ni.EnableStaticIPv4(
                                            ni.IPv4Address,
                                            ni.IPv4SubnetMask,
                                            ni.IPv4GatewayAddress);

                                        // check if static DNS is to be configured
                                        if (ni.IPv4DnsAddresses.Length > 0)
                                        {
                                            ni.EnableStaticIPv4Dns(ni.IPv4DnsAddresses);
                                        }
                                    }
                                }

                                _wifi.Connect(_ssid, _reconnectionKind, _password);

                                break;
                            }
                        }
                    }
                }

                return NetworkHelper.InternalWaitNetworkAvailable(
                    _workingNetworkInterface,
                    ref _networkHelperStatus,
                    false,
                    token,
                    setDateTime);
            }
            catch (Exception ex)
            {
                _networkHelperStatus = NetworkHelperStatus.ExceptionOccurred;
                HelperException = ex;

                return false;
            }
        }

        private static void StoreWifiConfiguration(NetworkInterface ni)
        {
            Wireless80211Configuration wc = Wireless80211Configuration.GetAllWireless80211Configurations()[ni.SpecificConfigId];

            // Let's make sure this configuration is saved
            wc.Ssid = _ssid;
            wc.Password = _password;

            wc.SaveConfiguration();
        }

        private static void WifiAvailableNetworksChanged(WifiAdapter sender, object e)
        {
            Debug.WriteLine("Wifi_AvailableNetworksChanged - get report");

            // Get Report of all scanned Wifi networks
            WifiNetworkReport report = sender.NetworkReport;

            // Enumerate though networks looking for our network
            foreach (WifiAvailableNetwork net in report.AvailableNetworks)
            {
                // Show all networks found
                Debug.WriteLine($"Net SSID :{net.Ssid},  BSSID : {net.Bsid},  rssi : {net.NetworkRssiInDecibelMilliwatts},  signal : {net.SignalBars}");

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

                    // sanity check
                    if (ni == null)
                    {
                        return;
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
                    WifiConnectionResult result = sender.Connect(net, _reconnectionKind, _password);

                    // Display status
                    if (result.ConnectionStatus == WifiConnectionStatus.Success)
                    {
                        sender.AvailableNetworksChanged -= WifiAvailableNetworksChanged;
                        StoreWifiConfiguration(ni);
                        return;
                    }
                    else
                    {
                        _networkHelperStatus = NetworkHelperStatus.ErrorConnetingToWiFiWhileScanning;
                    }
                }
            }
        }

        private static void WorkingThread()
        {
            // check if we have an IP
            if (!NetworkHelperInternal.CheckIP(
                _workingNetworkInterface,
                _ipConfiguration))
            {
                // wait here until we have an IP address
                _ipAddressAvailable.WaitOne();
            }

            if (_requiresDateTime)
            {
                // wait until there is a valid DateTime
                NetworkHelperInternal.WaitForValidDateTime();
            }

            // all conditions met
            _networkReady.Set();

            // update flag
            _networkHelperStatus = NetworkHelperStatus.NetworkIsReady;
        }

        /// <summary>
        /// Perform setup of the various fields and events, along with any of the required event handlers.
        /// </summary>
        /// <param name="setupEvents">Set true to setup the events. Required for the thread approach. Not required for the CancelationToken implementation.</param>
        private static void SetupHelper(bool setupEvents)
        {
            if (_helperInstanciated)
            {
                throw new InvalidOperationException();
            }
            else
            {
                // set flag
                _helperInstanciated = true;

                // flag to connect to Wifi after IP setup
                bool connectToWifi = false;

                // setup event
                _ipAddressAvailable = new(false);


                // currently we only support one Wifi adapter, so this is it
                _wifi = WifiAdapter.FindAllAdapters()[0];

                NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();

                if (setupEvents)
                {
                    // check if there are any network interface setup
                    if (nis.Length == 0)
                    {
                        _networkHelperStatus = NetworkHelperStatus.FailedNoNetworkInterface;

                        throw new NotSupportedException();
                    }

                    // setup handler
                    NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
                }

                if (!string.IsNullOrEmpty(_ssid) &&
                    !string.IsNullOrEmpty(_password))
                {
                    bool isAlreadyConnected = false;

                    // this is to connect to a specific Wifi network
                    // check if device it's already connected to the correct network
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

                    if (!isAlreadyConnected)
                    {
                        nis = NetworkInterface.GetAllNetworkInterfaces();

                        foreach (NetworkInterface ni in nis)
                        {
                            if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                            {
                                _wifi.Disconnect();

                                // Make sure we store the configuration
                                StoreWifiConfiguration(ni);

                                // set flag to connect to Wifi after IP config
                                connectToWifi = true;

                                // done here
                                break;
                            }
                        }
                    }

                }

                NetworkHelperInternal.InternalSetupHelper(
                    nis,
                    _workingNetworkInterface,
                    _ipConfiguration);

                if (connectToWifi)
                {
                    // connect to Wifi
                    _wifi.Connect(
                        _ssid,
                        _reconnectionKind,
                        _password);
                }

                // update status
                _networkHelperStatus = NetworkHelperStatus.Started;
            }
        }

        private static void AddressChangedCallback(object sender, EventArgs e)
        {
            if (NetworkHelperInternal.CheckIP(
                _workingNetworkInterface,
                _ipConfiguration))
            {
                _ipAddressAvailable.Set();
            }
        }

        /// <summary>
        /// Method to reset internal fields to it's defaults
        /// ONLY TO BE USED BY UNIT TESTS
        /// </summary>
        internal static void ResetInstance()
        {
            _ipAddressAvailable = null;
            _networkReady = new(false);
            _requiresDateTime = false;
            _networkHelperStatus = NetworkHelperStatus.None;
            _helperException = null;
            _ipConfiguration = null;
            _helperInstanciated = false;
            _ssid = null;
            _password = null;
            NetworkHelper.ResetInstance();
        }
    }
}
