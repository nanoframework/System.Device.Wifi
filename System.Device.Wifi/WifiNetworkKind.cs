//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace System.Device.Wifi
{
    /// <summary>
    /// Describes the kinds of Wifi networks.
    /// </summary>
    public enum WifiNetworkKind
    {
        /// <summary>
        /// An independent (IBSS) network.
        /// </summary>
        Adhoc,

        /// <summary>
        /// Either an infrastructure or independent network.
        /// </summary>
        Any,

        /// <summary>
        /// An infrastructure network.
        /// </summary>
        Infrastructure
    }
}
