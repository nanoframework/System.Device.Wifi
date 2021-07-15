// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.Networking
{
    /// <summary>
    /// IP configuration used for static IP address configuration.
    /// </summary>
    public class IPConfiguration

    {
        /// <summary>
        /// Constructor for IP Configuration.
        /// </summary>
        /// <param name="ipAddress">The static IP address.</param>
        /// <param name="ipSubnetMask">The IP subnet mask address.</param>
        /// <param name="ipGatewayAddress">The IP gateway address.</param>
        /// <param name="ipDns">The IP DNS list, null to let automatic DNS on.</param>
        public IPConfiguration(string ipAddress, string ipSubnetMask, string ipGatewayAddress, string[] ipDns)
        {
            IPAddress = ipAddress;
            IPSubnetMask = ipSubnetMask;
            IPGatewayAddress = ipGatewayAddress;
            IPDns = ipDns;
        }

        /// <summary>
        /// Gets or sets the static IP Address.
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the IP subnet mask address.
        /// </summary>
        public string IPSubnetMask { get; set; }

        /// <summary>
        /// Gets or sets the he IP gateway address.
        /// </summary>
        public string IPGatewayAddress { get; set; }

        /// <summary>
        /// Gets or sets the static DNS. null to let the automatic DNS enabled.
        /// </summary>
        public string[] IPDns { get; set; }
    }
}
