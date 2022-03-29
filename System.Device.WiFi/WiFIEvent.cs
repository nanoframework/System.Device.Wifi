//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;

namespace System.Device.WiFi
{
    [Flags]
    internal enum WiFiEventType : byte
    {
        // WiFi scan complete
        ScanComplete = 1
    }

    internal class WiFiEvent : BaseEvent
    {
        public WiFiEventType EventType;
        public DateTime Time;
    }
}
