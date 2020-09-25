//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using nanoFramework.Runtime.Events;

namespace Windows.Devices.WiFi
{
    [Flags]
    internal enum WiFiEventType : byte
    {
        ScanComplete = 1             // WiFI scan complete
    }

    internal class WiFiEvent : BaseEvent
    {
        public WiFiEventType EventType;
        public DateTime Time;
    }
}
