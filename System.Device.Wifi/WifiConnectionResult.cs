//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace System.Device.Wifi
{

    /// <summary>
    /// Describes the results of an attempt to connect to a Wifi network.
    /// </summary>
    public class WifiConnectionResult
    {
        private readonly WifiConnectionStatus _ConnectionStatus;

        internal WifiConnectionResult(WifiConnectionStatus ConnectionStatus)
        {
            _ConnectionStatus = ConnectionStatus;
        }

        /// <summary>
        /// Gets the connection result value.
        /// </summary>
        public WifiConnectionStatus ConnectionStatus
        {
            get
            {
                return _ConnectionStatus;
            }
        }
    }
}
