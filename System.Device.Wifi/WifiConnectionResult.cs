//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace System.Device.WiFi
{

    /// <summary>
    /// Describes the results of an attempt to connect to a WiFi network.
    /// </summary>
    public class WiFiConnectionResult
    {
        private readonly WiFiConnectionStatus _ConnectionStatus;

        internal WiFiConnectionResult(WiFiConnectionStatus ConnectionStatus)
        {
            _ConnectionStatus = ConnectionStatus;
        }

        /// <summary>
        /// Gets the connection result value.
        /// </summary>
        public WiFiConnectionStatus ConnectionStatus
        {
            get
            {
                return _ConnectionStatus;
            }
        }
    }
}
