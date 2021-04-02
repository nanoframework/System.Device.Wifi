//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

namespace Windows.Devices.WiFi
{
    /// <summary>
    /// Describes an available Wi-Fi network.
    /// </summary>
    public class WiFiAvailableNetwork
    {
        internal string _bsid = "";
        internal string _ssid = "";
        internal sbyte  _rssi =  0;

        private WiFiNetworkKind _networkKind = WiFiNetworkKind.Any;

        /// <summary>
        /// Gets the MAC address of the access point.
        /// </summary>
        public string Bsid { get { return _bsid; } set { _bsid = value; } }


        /// <summary>
        /// Gets the SSID (name) of the network.
        /// </summary>
        public string Ssid { get { return _ssid; } set { _ssid = value; } }

        /// <summary>
        /// Gets a value describing the kind of network being described.
        /// </summary>
        public WiFiNetworkKind NetworkKind { get { return _networkKind; } }

        /// <summary>
        /// Gets the signal strength of the network in Ddm
        /// </summary>
        public double NetworkRssiInDecibelMilliwatts {
            get
            {
                return (double)_rssi;
            }
        }

        /// <summary>
        /// Gets the strength of the signal as a number of bars.
        /// </summary>
        public byte SignalBars {
            get {
                byte bars = 0;

                // Map Rssi to signal bars
                if (_rssi > -55)      bars = 4;  // High
                else if (_rssi > -75) bars = 3;  // Medium
                else if (_rssi > -85) bars = 2;  // Low
                else if (_rssi > -96) bars = 1;  // Unusable 

                return bars;
            }
        }

    }
}
