using System;
using System.Collections.Generic;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Tests.CQRS
{
    public static class CqrsEventHandler<M> where M: class, IModel, new()
    {
        private static Queue<CqrsEvent<M>> EventQueue = new Queue<CqrsEvent<M>>();
        private static List<Action<CqrsEvent<M>>> SubscriberActions = new List<Action<CqrsEvent<M>>>();


        // Subscribing means: Execute the given action whenever
        // a new event is added to the eventQueue. 
        public static void Subscribe(Action<CqrsEvent<M>> subscribeAction)
        {
            SubscriberActions.Add(subscribeAction);
        }

        public static void AddEvent(CqrsEvent<M> newCqrsEvent)
        {
            EventQueue.Enqueue(newCqrsEvent);

            // Triggering the subscriber actions
            SubscriberActions.ForEach(x => x.Invoke(newCqrsEvent));
        }
    }

    
}
