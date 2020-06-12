//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using nanoFramework.Runtime.Events;

namespace Windows.Devices.WiFi
{
    /// <summary>
    /// Event raised when a scan completes on this Wi-Fi adapter. 
    /// </summary>
    public delegate void AvailableNetworksChangedEventHandler(
                            WiFiAdapter sender,
                            Object e);


    /// <summary>
    /// Provides a means to scan for nearby WiFi access points, enumerate those that are found, and connect to an access point.
    /// </summary>
    public sealed class WiFiAdapter : IDisposable
    {
        private int _networkInterface;

        private static WiFiEventListener s_eventListener = new WiFiEventListener();

        // This is used as the lock object 
        // a lock is required because multiple threads can access the WifiAdapter
        private object _syncLock = new object();

        /// <summary>
        /// Event raised when a scan completes on this Wi-Fi adapter. 
        /// </summary>
        public event AvailableNetworksChangedEventHandler AvailableNetworksChanged;

        internal void OnAvailableNetworksChangedInternal(WiFiEvent e)
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

        internal WiFiAdapter(int NetworkInterface)
        {
            _networkInterface = NetworkInterface;
            NativeInit();
            s_eventListener.AddAdapter(this);
        }

        /// <summary>
        /// Gets the network interface number associatted with this Wi-Fi adapter
        /// </summary>
        public int NetworkInterface
        {
            get { return _networkInterface; }
        }
        
        /// <summary>
        /// Gets a list of available networks populated by the last Wi-Fi scan on this WiFiNetworkAdapter.
        /// </summary>
        public WiFiNetworkReport NetworkReport {
            get
            {
                lock (_syncLock)
                {
                    return new WiFiNetworkReport ( ParseNativeReports( GetNativeScanReport() ) );
                }
            }
        }

        private WiFiAvailableNetwork[] ParseNativeReports(byte[] nativeReport)
        {
            int bytePos = 1;
            int recordCount = nativeReport[0];

            WiFiAvailableNetwork[] WifiNetworks = new WiFiAvailableNetwork[recordCount];
            for (int index = 0; index < recordCount; index++)
            {
                WifiNetworks[index] = new WiFiAvailableNetwork();

                WifiNetworks[index].Bsid = BitConverter.ToString(nativeReport, bytePos, 6);
                bytePos += 6;

                // need to convert this programmatically to prevent referencing System.Text
                char[] rawSsid = new char[33];
                for(int i = 0; i < 33; i++)
                {
                    rawSsid[i] = (char)nativeReport[bytePos + i];
                }

                WifiNetworks[index].Ssid = new string(rawSsid, 0, 33);
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
        /// Connect this Wi-Fi device to the specified network, with the specified passphrase and reconnection policy.
        /// </summary>
        /// <param name="availableNetwork">Describes the Wi-Fi network to be connected.</param>
        /// <param name="reconnectionKind">Specifies how to reconnect if the connection is lost.</param>
        /// <param name="passwordCredential">The passphrase to be used to connect to the access point.</param>
        /// <returns>
        /// On successful conclusion of the operation, returns an object that describes the result of the connect operation.
        /// </returns>
        public WiFiConnectionResult Connect(WiFiAvailableNetwork availableNetwork, WiFiReconnectionKind reconnectionKind, string passwordCredential)
        {
            lock (_syncLock)
            {
                return new WiFiConnectionResult( NativeConnect(availableNetwork.Ssid, passwordCredential, reconnectionKind) );
            }
        }


        /// <summary>
        /// Disconnects any active Wi-Fi connection through this adapter.
        /// </summary>
        public void Disconnect()
        {
            NativeDisconnect();
        }

 
        /// <summary>
        /// A static method that enumerates all the Wi-Fi adapters in the system.
        /// </summary>
        /// <returns>>On successful completion, returns an array of WiFiAdapter objects</returns>
        public static WiFiAdapter[] FindAllAdapters()
        {
            int index = 0;

            // Get array of available Wireless Adapters interface indexes in system
            byte[] adapterIndexes = NativeFindWirelessAdapters();
            WiFiAdapter[] adapters = new WiFiAdapter[adapterIndexes.Length];

            foreach ( byte adapterIndex in adapterIndexes)
            {
                adapters[index++] = new WiFiAdapter(adapterIndex);
            }

            return adapters;
        }


        /// <summary>
        /// Directs this adapter to initiate an asynchronous network scan.
        /// </summary>
        /// <remarks>On successful completion, returns a list of networks scanned by this adapter signalled by the AvailableNetworksChanged event.
        /// Use NetworkReport to retrive the list of available Networks.
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
            if (! _disposedValue)
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
        ~WiFiAdapter()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose WiFiAdapter
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

        #region extenal calls to native implementations

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void DisposeNative();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInit();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern byte[] NativeFindWirelessAdapters();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern WiFiConnectionStatus NativeConnect(string Ssid, string passwordCredential, WiFiReconnectionKind reconnectionKind);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeDisconnect();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeScanAsync();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern byte[] GetNativeScanReport();

        #endregion

    }
}
