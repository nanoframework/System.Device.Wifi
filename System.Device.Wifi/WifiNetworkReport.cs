//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace System.Device.Wifi
{
    /// <summary>
    /// Contains the result of a network scan operation.
    /// </summary>
    public class WifiNetworkReport
    {
        readonly WifiAvailableNetwork[] _WifiNetworks;

        internal WifiNetworkReport(WifiAvailableNetwork[] WifiNetworks)
        {
            _WifiNetworks = WifiNetworks;
        }

        /// <summary>
        /// A list of available networks.
        /// </summary>
        public WifiAvailableNetwork[] AvailableNetworks
        {
            get
            {
                return _WifiNetworks;
            }
        }
    }
}
