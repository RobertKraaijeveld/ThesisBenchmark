using System.Collections.Generic;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Tests.CQRS
{
    public class CqrsReader<M> where M: class, IModel, new()
    {
        private readonly IDatabaseApi api;
        public CrudModels<M> crudModels;

        public CqrsReader(IDatabaseApi api, CrudModels<M> crudModels) 
        {
            this.api = api;
            this.crudModels = crudModels;

            // Subscribing to event handler
            CqrsEventHandler<M>.Subscribe(cqrsEvent =>
            {
                if (cqrsEvent.EventType.Equals(ECqrsEventType.CreateEvent))
                {
                    // When the CqrsWriter has added a model through THEIR database, we add it in OUR database.
                    api.Create(new List<M>() { (M)cqrsEvent.Model }, crudModels.CreateModel);
                }
                else if (cqrsEvent.EventType.Equals(ECqrsEventType.DeleteEvent))
                {
                    // When the CqrsWriter has removed a model from THEIR database, we remove it from OUR database.
                    api.Delete<M>(new List<M>(){(M) cqrsEvent.Model}, crudModels.DeleteModel);
                }
            });            
        }

        public IEnumerable<M> GetAll(IGetAllModel<M> getAllModel)
        {
            return api.GetAll(getAllModel);
        }

        public IEnumerable<M> Search(ISearchModel<M> searchModel) 
        {
            return api.Search(searchModel);
        }
    }
}
