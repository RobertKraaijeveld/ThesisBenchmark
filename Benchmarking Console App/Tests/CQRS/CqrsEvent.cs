using Benchmarking_Console_App.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Tests.CQRS
{
    public class CqrsEvent<M> where M: class, IModel, new()
    {
        public readonly ECqrsEventType EventType;
        public readonly IModel Model;

        public CqrsEvent(ECqrsEventType eventType, IModel model)
        {
            this.EventType = eventType;
            this.Model = model;
        }
    }

    public enum ECqrsEventType
    {
        CreateEvent,
        UpdateEvent,
        DeleteEvent
    }
}
