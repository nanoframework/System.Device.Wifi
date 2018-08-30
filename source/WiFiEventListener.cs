using System;
using System.Collections;
using nanoFramework.Runtime.Events;

namespace Windows.Devices.WiFi
{
    internal class WiFiEventListener : IEventProcessor, IEventListener
    {
        // FIXME currently supports 1 adapter
        System.Collections.ArrayList wifiAdapters = new ArrayList();
        //WiFiAdapter wifiAdapter;

        public WiFiEventListener()
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
        public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time)
        {
            WiFiEventType eventType = (WiFiEventType)(data1 & 0xFF);
            if (eventType >= WiFiEventType.ScanComplete)
            {
                WiFiEvent wifiEvent = new WiFiEvent();
                wifiEvent.EventType = eventType;
                wifiEvent.Time = time;

                return wifiEvent;
            }
            return null;
        }

        public bool OnEvent(BaseEvent ev)
        {
            if (ev is WiFiEvent)
            {
                foreach( object obj in wifiAdapters)
                {
                    WiFiAdapter wifiAdapter = obj as WiFiAdapter;
                    wifiAdapter.OnAvailableNetworksChangedInternal((WiFiEvent)ev);
                }

                return true;
            }
            return false;
        }
        public void InitializeForEventSource()
        {
        }

        internal void AddAdapter(WiFiAdapter adapter)
        {
            wifiAdapters.Add(adapter);
        }

        internal void RemoveAdapter(WiFiAdapter wifi)
        {
            wifiAdapters.Remove(wifi);
        }
    }
}
