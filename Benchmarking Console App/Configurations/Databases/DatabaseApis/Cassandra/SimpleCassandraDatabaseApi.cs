using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;
using Cassandra;
using Cassandra.Mapping;


namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    /// <summary>
    /// Provides basic CRUD functionality for the Cassandra database.
    /// </summary>
    public class SimpleCassandraDatabaseApi : IDatabaseApi
    {
        private readonly string _connectionString;
        private readonly string KEYSPACE_NAME = "benchmarkdb";

        private readonly Cluster _cassandraCluster;
        private readonly ISession _cassandraSession;
        private readonly IMapper _cassandraMapper;


        public SimpleCassandraDatabaseApi(string connectionString)
        {
            _connectionString = connectionString;

            var builder = Cluster.Builder().WithConnectionString(_connectionString);

            _cassandraCluster = builder.Build();
            _cassandraSession = _cassandraCluster.Connect(KEYSPACE_NAME);
            _cassandraMapper = new Mapper(_cassandraSession);
        }


        public IEnumerable<M> Get<M>(string collectionName, ISearchModel searchModel) where M : IModel, new()
        {
            var queryText = searchModel.GetSearchString(collectionName);
            return _cassandraMapper.Fetch<M>(queryText);
        }

        public void Create<M>(IEnumerable<M> newModels, ICreateModel createModel) where M : IModel, new()
        {
            foreach (var model in newModels)
            {
                var createQueryText = createModel.GetCreateString(model);
                _cassandraMapper.Execute(createQueryText);
            }
        }

        public void Update<M>(IEnumerable<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new()
        {
            foreach (var model in modelsWithNewValues)
            {
                var updateQueryText = updateModel.GetUpdateString(model);
                _cassandraMapper.Execute(updateQueryText);
            }
        }

        public void Delete<M>(IEnumerable<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new()
        {
            foreach (var model in modelsToDelete)
            {
                var updateQueryText = deleteModel.GetDeleteString(model);
                _cassandraMapper.Execute(updateQueryText);
            }
        }

        private void PutQuotesAroundTableName(string queryText)
        {

        }
    }
}
