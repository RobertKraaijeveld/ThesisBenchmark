using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;
using Newtonsoft.Json;
using StackExchange.Redis;


namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    /// <summary>
    /// Provides basic CRUD functionality for the Redis KV-database.
    /// Each KV in the DB is: { Key: a string key, Value: A JSON representation of an IModel }
    /// </summary>
    public class SimpleRedisDatabaseApi : IDatabaseApi
    {
        private readonly string _connectionString;
        private readonly ConnectionMultiplexer _connection;

        public SimpleRedisDatabaseApi(string connectionString)
        {
            _connectionString = connectionString;
            _connection = ConnectionMultiplexer.Connect(connectionString);
        }

        
        public IEnumerable<M> Get<M>(string collectionName, ISearchModel searchModel) where M : IModel, new()
        {
            IDatabase db = _connection.GetDatabase();
            var commandText = searchModel.GetSearchString(collectionName: "not needed"); // TODO: Ugly param solution

            var resultJson = db.StringGet(commandText).ToString();
            return JsonConvert.DeserializeObject<IEnumerable<M>>(resultJson);
        }

        public void Create<M>(IEnumerable<M> newModels, ICreateModel createModel) where M : IModel, new()
        {
            IDatabase db = _connection.GetDatabase();

            foreach (var model in newModels)
            { 
                db.Execute(createModel.GetCreateString(model));
            }
        }

        public void Update<M>(IEnumerable<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new()
        {
            IDatabase db = _connection.GetDatabase();

            foreach (var model in modelsWithNewValues)
            {
                db.Execute(updateModel.GetUpdateString(model));
            }

        }

        public void Delete<M>(IEnumerable<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new()
        {
            IDatabase db = _connection.GetDatabase();

            foreach (var model in modelsToDelete)
            {
                db.Execute(deleteModel.GetDeleteString(model));
            }
        }
    }
}
