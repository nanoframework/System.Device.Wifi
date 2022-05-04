//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;

namespace System.Device.Wifi
{
    [Flags]
    internal enum WifiEventType : byte
    {
        // Wifi scan complete
        ScanComplete = 1
    }

    internal class WifiEvent : BaseEvent
    {
        public WifiEventType EventType;
        public DateTime Time;
    }
}
