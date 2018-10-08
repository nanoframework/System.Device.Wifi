//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

using System.Collections;

namespace Windows.Devices.WiFi
{
    /// <summary>
    /// Contains the result of a network scan operation.
    /// </summary>
    public class WiFiNetworkReport 
    {
        WiFiAvailableNetwork[] _wifiNetworks;

        internal WiFiNetworkReport(WiFiAvailableNetwork[] WifiNetworks )
        {
            _wifiNetworks = WifiNetworks;
        }

        /// <summary>
        /// A list of available networks.
        /// </summary>
        public WiFiAvailableNetwork[] AvailableNetworks {
            get
            {
                return _wifiNetworks;
            }
        }
    }
}
