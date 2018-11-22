using System.Collections.Generic;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Tests.CQRS
{
    public class CqrsWriter<M> where M : class, IModel, new()
    {
        private readonly IDatabaseApi api;
        public CrudModels<M> crudModels;

        public CqrsWriter(IDatabaseApi api, CrudModels<M> crudModels)
        {
            this.api = api;
            this.crudModels = crudModels;
        }


        public void OpenConnectionToApi()
        {
            this.api.OpenConnection();
        }

        public void CloseConnectionToApi()
        {
            this.api.CloseConnection();
        }


        public void Create(List<M> newModels) 
        {
            api.Create(newModels, crudModels.CreateModel);

            foreach (var newModel in newModels)
            {
                var eventForNewModel = new CqrsEvent<M>(ECqrsEventType.CreateEvent, newModel);
                CqrsEventHandler<M>.AddEvent(eventForNewModel);
            }
        }

        public void Update<M>(List<M> modelsWithNewValues) where M : class, IModel, new()
        {
           api.Update(modelsWithNewValues, this.crudModels.UpdateModel);

           foreach (var model in modelsWithNewValues)
           {
                var eventForUpdatedModel = new CqrsEvent<M>(ECqrsEventType.UpdateEvent, model);
                CqrsEventHandler<M>.AddEvent(eventForUpdatedModel);
           }
        }

        public void Delete(List<M> modelsToDelete) 
        { 
            api.Delete(modelsToDelete, crudModels.DeleteModel);

            foreach (var deletedModel in modelsToDelete)
            {
                var eventForDeletedModel = new CqrsEvent<M>(ECqrsEventType.DeleteEvent, deletedModel);
                CqrsEventHandler<M>.AddEvent(eventForDeletedModel);
            }
        }

        public void Truncate()
        {
            api.Truncate<M>();
        }
    }
}
