//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Text;

namespace Windows.Devices.WiFi
{
    //Duplicate of AuthenticationType enum from nanoFramework.System.Net
    /// <summary>
    /// Specifies the authentication used in a wireless network.
    /// </summary>
    public enum AuthenticationType : byte
    {
        /// <summary>
        /// No protocol.
        /// </summary>
        None = 0,

        /// <summary>
        /// Extensible Authentication Protocol.
        /// </summary>
        EAP,

        /// <summary>
        /// Protected Extensible Authentication Protocol.
        /// </summary>
        PEAP,

        /// <summary>
        /// Microsoft Windows Connect Now protocol.
        /// </summary>
        WCN,

        /// <summary>
        /// Open System authentication, for use with WEP encryption type.
        /// </summary>
        Open,

        /// <summary>
        /// Shared Key authentication, for use with WEP encryption type.
        /// </summary>
        Shared,

        /// <summary>
        /// Wired Equivalent Privacy protocol.
        /// </summary>
        WEP,

        /// <summary>
        /// Wi-Fi Protected Access protocol.
        /// </summary>
        WPA,

        /// <summary>
        /// Wi-Fi Protected Access 2 protocol.
        /// </summary>
        WPA2,
    }

    /// <summary>        
    /// Represents an access point. The object provides methods and properties that an app can use to manage access point.        
    /// </summary>    
    public sealed class WiFiAccessPoint : IDisposable
    {        
        private string _ssid="";
        private string _passwordCredential="";
        private int _channel=11;
        private int _maxConnections=1;
        private bool _hiddeSSID=false;
        private AuthenticationType _authenticationType = AuthenticationType.Open; // Needed a local copy of Enum, otherwise needed reference to the nanoFramework.System.Net.

        /// <summary>
        /// Event raised when a clients connected or disconnected from AP
        /// </summary>        
        public event NetworkAPStationChangedEventHandler NetworkAPStationChanged;
       //or public event NetworkAPDeviceChangedEventHandler NetworkAPDeviceChanged;

        public WifiAPOpenResult Open()
        {
            return Open("", "", _channel, _maxConnections, _hiddeSSID);
        }

        public WifiAPOpenResult Open(string ssid)
        {
            return Open(ssid, "", _channel, _maxConnections, _hiddeSSID);
        }

        public WifiAPOpenResult Open(string ssid, string passwordCredential)
        {
            return Open(ssid, passwordCredential, _channel, _maxConnections, _hiddeSSID);
        }
    
        public WifiAPOpenResult Open(string ssid, string passwordCredential, int channel)
        {
            return Open(ssid, passwordCredential, channel, _maxConnections, _hiddeSSID);
        }

        public WifiAPOpenResult Open(string ssid, string passwordCredential, int channel, int maxConnections)
        {            
            return Open(ssid, passwordCredential, channel, maxConnections, _hiddeSSID);
        }

        public WifiAPOpenResult Open(string ssid, string passwordCredential, int channel, int maxConnections, bool hiddeSSID)
        {
            _ssid = ssid;
            _passwordCredential = passwordCredential;
            _channel = channel;
            _maxConnections = maxConnections;
            _hiddeSSID = hiddeSSID;

            return WifiOpenAP();
        }

        public WifiAPOpenResult Close()
        {
            //Call to Native AP Close
        }

        public int NetworkInterface
        {
            //We need access to NetworkInterface in case of adjusting IP settings.
            get { return _networkInterface; }
        }

        /// <summary>
        /// A method that enumerates all connected Wifi Stations.
        /// </summary>
        /// <returns>>On successful completion, returns an array of WiFiStation objects</returns>
        public WifiStations[] FindAllStations()
        { 
        
        }
        ////Or just Get property 
        //readonly public WifiStations[] GetWifiStations
        //{
        //    get
        //    {
        //        return null;
        //    }
        //}

        private WifiAPOpenResult WifiOpenAP()
        {
            if (_ssid = "")
            {
                _ssid = "nano_"; // + mac address           
            }

            if (passwordCredential = "" || _passwordCredential.Length<8)
            {
                _authenticationType = AuthenticationType.Open;
            }
            else
            {
                _authenticationType = AuthenticationType.WPA2;
            }

            if (_channel > 0 && _channel < 11)
            {
                _channel = 11;
            }

            //All private variables is set and now is possible to setup AP
        }
    }
}