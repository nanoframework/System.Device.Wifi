//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace System.Device.Wifi
{
    /// <summary>
    /// Describes whether to automatically reconnect to this network.
    /// </summary>
    public enum WifiReconnectionKind
    {
        /// <summary>
        /// Reconnect automatically.
        /// </summary>
        Automatic,

        /// <summary>
        /// Allow user to reconnect manually.
        /// </summary>
        Manual
    }
}
