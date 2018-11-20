using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Tests.CQRS
{
    public class CqrsEvent
    {
        public readonly CqrtsEventType EventType;
        public readonly IModel model;

        public CqrsEvent(CqrtsEventType eventType, IModel model)
        {
            this.EventType = eventType;
            this.model = model;
        }
    }

    public enum CqrtsEventType
    {
        CreateEvent,
        UpdateEvent,
        DeleteEvent
    }
}
