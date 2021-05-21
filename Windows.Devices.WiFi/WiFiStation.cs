using System;

namespace Windows.Devices.WiFi
{
    //Duplicate of PhyProtocols enum from nanoFramework.System.Net
    /// <summary>
    /// Enum of Phy protocols used for connection.
    /// </summary>
    [Flags]
    public enum PhyProtocols
    {
        /// <summary>
        /// IEEE 802.11b  max 11 Mbit/s
        /// </summary>
        PHY802_11b = 1,

        /// <summary>
        /// IEEE 802.11g  max 54 Mbit/s
        /// </summary>
        PHY802_11g = 2,

        /// <summary>
        /// IEEE 802.11n  max 288.8 Mbit/s for 20mhz channel or 600 for 40Mhz
        /// </summary>
        PHY802_11n = 4,

        /// <summary>
        /// Low rate enabled.
        /// </summary>
        PHY802_11lr = 8,
    };

    //Duplicate of WirelessAPStation class from nanoFramework.System.Net
    public class WiFiStation
    {
        public byte[] MacAddres;
        public sbyte Rssi;
        public PhyProtocols PhyModes;
        //Not sure, but I didn't find any low level code for the IP address of the station info.
    }
}