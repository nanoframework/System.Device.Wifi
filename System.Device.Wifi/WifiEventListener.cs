//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;
using System.Collections;

namespace System.Device.Wifi
{
    internal class WifiEventListener : IEventProcessor, IEventListener
    {
        readonly ArrayList WifiAdapters = new();

        public WifiEventListener()
        {
            EventSink.AddEventProcessor(EventCategory.WiFi, this);
            EventSink.AddEventListener(EventCategory.WiFi, this);
        }

        /// <summary>
        /// Process an event
        /// </summary>
        /// <param name="data1"> bits 0-8 = subCategory, bits 9-15=category, bits 16-31=data1 </param>
        /// <param name="data2"> data2 </param>
        /// <param name="time"></param>
        /// <returns></returns>
        public BaseEvent ProcessEvent(
            uint data1,
            uint data2,
            DateTime time)
        {
            WifiEventType eventType = (WifiEventType)(data1 & 0xFF);
            if (eventType >= WifiEventType.ScanComplete)
            {
                WifiEvent WifiEvent = new WifiEvent();
                WifiEvent.EventType = eventType;
                WifiEvent.Time = time;

                return WifiEvent;
            }
            return null;
        }

        public bool OnEvent(BaseEvent ev)
        {
            if (ev is WifiEvent)
            {
                foreach (object obj in WifiAdapters)
                {
                    WifiAdapter WifiAdapter = obj as WifiAdapter;
                    WifiAdapter.OnAvailableNetworksChangedInternal((WifiEvent)ev);
                }

                return true;
            }
            return false;
        }

        public void InitializeForEventSource()
        {
            // need this here to match base class
        }

        internal void AddAdapter(WifiAdapter adapter)
        {
            WifiAdapters.Add(adapter);
        }

        internal void RemoveAdapter(WifiAdapter Wifi)
        {
            WifiAdapters.Remove(Wifi);
        }
    }
}
