using System.Collections.Generic;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
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

        public void Create(IEnumerable<M> newModels) 
        {
            api.Create(newModels, crudModels.CreateModel);

            foreach (var newModel in newModels)
            {
                var eventForNewModel = new CqrsEvent<M>(ECqrsEventType.CreateEvent, newModel);
                CqrsEventHandler<M>.AddEvent(eventForNewModel);
            }
        }

        // TODO: IMPLEMENT LATER
        //public void Update<M, P>(IEnumerable<M> modelsWithNewValues, 
        //                         IEnumerable<P> oldModelPrimaryKeys, 
        //                         IUpdateModel updateModel) where M : IModel, new()
        //{
        //    api.Update(modelsWithNewValues, updateModel);

        //    foreach (var updatedModel in modelsWithNewValues)
        //    {

        //    }
        //}

        public void Delete(IEnumerable<M> modelsToDelete) 
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
