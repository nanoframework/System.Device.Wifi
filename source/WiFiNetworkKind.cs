//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

namespace Windows.Devices.WiFi
{
    /// <summary>
    /// Describes the kinds of Wi-Fi networks.
    /// </summary>
    public enum WiFiNetworkKind
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
