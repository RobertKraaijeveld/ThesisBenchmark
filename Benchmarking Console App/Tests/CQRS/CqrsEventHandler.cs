using System;
using System.Collections.Generic;

namespace Benchmarking_Console_App.Tests.CQRS
{
    public static class CqrsEventHandler
    {
        private static Queue<CqrsEvent> EventQueue = new Queue<CqrsEvent>();
        private static List<Action<CqrsEvent>> SubscriberActions = new List<Action<CqrsEvent>>();


        // Subscribing means: Execute the given action whenever
        // a new event is added to the eventQueue. 
        public static void Subscribe(Action<CqrsEvent> subscribeAction)
        {
            SubscriberActions.Add(subscribeAction);
        }

        public static void AddEvent(CqrsEvent newCqrsEvent)
        {
            EventQueue.Enqueue(newCqrsEvent);

            // Triggering the subscriber actions
            SubscriberActions.ForEach(x => x.Invoke(newCqrsEvent));
        }
    }

    
}
