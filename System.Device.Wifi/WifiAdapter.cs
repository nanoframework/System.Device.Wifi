//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Device.Wifi
{
    /// <summary>
    /// Event raised when a scan completes on this Wifi adapter. 
    /// </summary>
    public delegate void AvailableNetworksChangedEventHandler(
                            WifiAdapter sender,
                            Object e);


    /// <summary>
    /// Provides a means to scan for nearby Wifi access points, enumerate those that are found, and connect to an access point.
    /// </summary>
    public sealed class WifiAdapter : IDisposable
    {
        private readonly int _networkInterface;

        private static WifiEventListener s_eventListener = new WifiEventListener();

        // This is used as the lock object 
        // a lock is required because multiple threads can access the WifiAdapter
        private readonly object _syncLock = new object();

        /// <summary>
        /// Event raised when a scan completes on this Wifi adapter. 
        /// </summary>
        public event AvailableNetworksChangedEventHandler AvailableNetworksChanged;

        internal void OnAvailableNetworksChangedInternal(WifiEvent e)
        {
            AvailableNetworksChangedEventHandler callbacks = null;

            lock (_syncLock)
            {
                if (!_disposedValue)
                {
                    callbacks = AvailableNetworksChanged;
                }
            }

            callbacks?.Invoke(this, new EventArgs());
        }

        internal WifiAdapter(int NetworkInterface)
        {
            _networkInterface = NetworkInterface;
            NativeInit();
            s_eventListener.AddAdapter(this);
        }

        /// <summary>
        /// Gets the network interface number associatted with this Wifi adapter
        /// </summary>
        public int NetworkInterface
        {
            get { return _networkInterface; }
        }

        /// <summary>
        /// Gets a list of available networks populated by the last Wifi scan on this WifiNetworkAdapter.
        /// </summary>
        public WifiNetworkReport NetworkReport
        {
            get
            {
                lock (_syncLock)
                {
                    return new WifiNetworkReport(ParseNativeReports(GetNativeScanReport()));
                }
            }
        }

        private WifiAvailableNetwork[] ParseNativeReports(byte[] nativeReport)
        {
            int bytePos = 1;
            int recordCount = nativeReport[0];

            WifiAvailableNetwork[] WifiNetworks = new WifiAvailableNetwork[recordCount];
            for (int index = 0; index < recordCount; index++)
            {
                WifiNetworks[index] = new WifiAvailableNetwork();

                WifiNetworks[index].Bsid = BitConverter.ToString(nativeReport, bytePos, 6);
                bytePos += 6;

                byte[] rawSsid = new byte[33];
                Array.Copy(nativeReport, bytePos, rawSsid, 0, 33);

                int ssidLength = Array.IndexOf(rawSsid, (byte)0);
                if (ssidLength < 0)
                {
                    ssidLength = 33;
                }

                WifiNetworks[index].Ssid = Encoding.UTF8.GetString(rawSsid, 0, ssidLength);
                bytePos += 33;

                WifiNetworks[index]._rssi = (sbyte)nativeReport[bytePos];
                bytePos++;

                // FIXME - Not using Auth mode and Cypher Yet
                // Needs the Windows.Networking.Connectivity.NetworkEncryptionType &
                // Networking.Connectivity.AuthenticationType defined
                int authMode = (sbyte)nativeReport[bytePos];
                bytePos++;

                int cypherType = (sbyte)nativeReport[bytePos];
                bytePos++;
            }

            return WifiNetworks;
        }

        /// <summary>
        /// Connect this Wifi device to the specified network, with the specified pass-phrase and reconnection policy.
        /// </summary>
        /// <param name="availableNetwork">Describes the Wifi network to be connected.</param>
        /// <param name="reconnectionKind">Specifies how to reconnect if the connection is lost.</param>
        /// <param name="passwordCredential">The pass-phrase to be used to connect to the access point.</param>
        /// <returns>
        /// On successful conclusion of the operation, returns an object that describes the result of the connect operation.
        /// </returns>
        public WifiConnectionResult Connect(
            WifiAvailableNetwork availableNetwork,
            WifiReconnectionKind reconnectionKind,
            string passwordCredential)
        {
            lock (_syncLock)
            {
                return new WifiConnectionResult(NativeConnect(
                    availableNetwork.Ssid,
                    passwordCredential,
                    reconnectionKind));
            }
        }

        /// <summary>
        /// Connect this Wifi device to the specified network (using SSID string), with the specified pass-phrase and reconnection policy.
        /// </summary>
        /// <param name="ssid">Describes the Wifi network to be connected.</param>
        /// <param name="reconnectionKind">Specifies how to reconnect if the connection is lost.</param>
        /// <param name="passwordCredential">The pass-phrase to be used to connect to the access point.</param>
        /// <returns>
        /// On successful conclusion of the operation, returns an object that describes the result of the connect operation.
        /// </returns>
        public WifiConnectionResult Connect(
            string ssid,
            WifiReconnectionKind reconnectionKind,
            string passwordCredential)
        {
            WifiAvailableNetwork availableNetwork = new();
            availableNetwork.Ssid = ssid;
            return Connect(availableNetwork, reconnectionKind, passwordCredential);
        }


        /// <summary>
        /// Disconnects any active Wifi connection through this adapter.
        /// </summary>
        public void Disconnect()
        {
            NativeDisconnect();
        }

        /// <summary>
        /// A static method that enumerates all the Wifi adapters in the system.
        /// </summary>
        /// <returns>>On successful completion, returns an array of WifiAdapter objects</returns>
        public static WifiAdapter[] FindAllAdapters()
        {
            int index = 0;

            // Get array of available Wireless Adapters interface indexes in system
            byte[] adapterIndexes = NativeFindWirelessAdapters();
            WifiAdapter[] adapters = new WifiAdapter[adapterIndexes.Length];

            foreach (byte adapterIndex in adapterIndexes)
            {
                adapters[index++] = new WifiAdapter(adapterIndex);
            }

            return adapters;
        }

        /// <summary>
        /// Sets the device name advertised by this Wi-Fi adapter when connecting to a network.
        /// </summary>
        /// <param name="deviceName">The device name to assign to this adapter.</param>
        /// <exception cref="ArgumentException">If <paramref name="deviceName"/> is <see langword="null"/>, empty or the length over 32 characters.</exception>
        /// <exception cref="NotSupportedException">Thrown when the current firmware was built without Wi-Fi and setting the device name is not supported on this platform.</exception>
        /// <exception cref="NotImplementedException">Thrown on platforms where setting the device name is not yet implemented.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the underlying Wi-Fi stack rejects to specify device name. On ESP32 this corresponds to a failure of the native <c>esp_netif_set_hostname</c> call.</exception>
        /// <remarks>
        /// The device name is sent to the access point during connection and may appear in the router’s
        /// client list or DHCP lease table.  
        /// This method must be called before initiating a connection to take effect.
        /// </remarks>
        public void SetDeviceName(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName) || deviceName.Length > 32)
            {
                throw new ArgumentException();
            }

            NativeSetDeviceName(deviceName);
        }

        /// <summary>
        /// Directs this adapter to initiate an asynchronous network scan.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the Wi-Fi interface hasn't been started.</exception>
        /// <exception cref="Busy">If the Wi-Fi interface is performing a connect operation.</exception>
        /// <exception cref="TimeoutException">If a timeout occurred when trying to initiate a scan operation.</exception>
        /// <exception cref="Fail">Failed to get the configuration for the Wi-Fi adapter.</exception>
        /// <remarks>On successful completion, returns a list of Wi-Fi networks scanned by this adapter signaled by the <see cref="AvailableNetworksChanged"/> event.
        /// Use <see cref="NetworkReport"/> to retrieve the list of available Wi-Fi networks.
        /// </remarks>
        public void ScanAsync()
        {
            lock (_syncLock)
            {
                NativeScanAsync();
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                s_eventListener.RemoveAdapter(this);
                DisposeNative();

                _disposedValue = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~WifiAdapter()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose WifiAdapter
        /// </summary>
        public void Dispose()
        {
            lock (_syncLock)
            {
                if (!_disposedValue)
                {
                    Dispose(true);

                    GC.SuppressFinalize(this);
                }
            }
        }
        #endregion

        #region external calls to native implementations

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void DisposeNative();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInit();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern byte[] NativeFindWirelessAdapters();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern WifiConnectionStatus NativeConnect(string Ssid, string passwordCredential, WifiReconnectionKind reconnectionKind);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeDisconnect();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeScanAsync();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern byte[] GetNativeScanReport();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetDeviceName(string deviceName);

        #endregion

    }
}
