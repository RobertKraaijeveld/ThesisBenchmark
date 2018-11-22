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
                    api.OpenConnection();
                    api.Create(new List<M>() { (M)cqrsEvent.Model }, crudModels.CreateModel);
                    api.CloseConnection();
                }
                else if (cqrsEvent.EventType.Equals(ECqrsEventType.DeleteEvent))
                {
                    // When the CqrsWriter has removed a model from THEIR database, we remove it from OUR database.
                    api.OpenConnection();
                    api.Delete<M>(new List<M>(){(M) cqrsEvent.Model}, crudModels.DeleteModel);
                    api.CloseConnection();
                }
            });            
        }

        public void OpenConnectionToApi()
        {
            this.api.OpenConnection();
        }

        public void CloseConnectionToApi()
        {
            this.api.CloseConnection();
        }
        

        public List<M> Search(List<ISearchModel<M>> searchModels) 
        {
            return api.Search(searchModels);
        }
    }
}
