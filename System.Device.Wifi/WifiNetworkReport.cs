//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace System.Device.WiFi
{
    /// <summary>
    /// Contains the result of a network scan operation.
    /// </summary>
    public class WiFiNetworkReport
    {
        readonly WiFiAvailableNetwork[] _wifiNetworks;

        internal WiFiNetworkReport(WiFiAvailableNetwork[] WifiNetworks)
        {
            _wifiNetworks = WifiNetworks;
        }

        /// <summary>
        /// A list of available networks.
        /// </summary>
        public WiFiAvailableNetwork[] AvailableNetworks
        {
            get
            {
                return _wifiNetworks;
            }
        }
    }
}
