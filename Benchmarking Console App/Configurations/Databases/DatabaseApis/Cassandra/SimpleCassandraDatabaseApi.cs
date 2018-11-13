using System.Collections.Generic;
using System.Linq;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
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
        public readonly string KEYSPACE_NAME = "benchmarkdb";

        private readonly string _connectionString;

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

        public IEnumerable<M> GetAll<M>(IGetAllModel<M> getAllModel) where M : IModel, new()
        {
            var getAllQueryText = getAllModel.CreateGetAllString();
            return _cassandraMapper.Fetch<M>(getAllQueryText).ToList();
        }

        public IEnumerable<M> Search<M>(ISearchModel<M> searchModel) where M : IModel, new()
        {
            var searchQueryText = searchModel.GetSearchString<M>();
            return _cassandraMapper.Fetch<M>(searchQueryText).ToList();
        }

        public int Amount<M>() where M : IModel, new()
        {
            int amount = 0;

            var allTableNames = this.GetAllTableNames();
            foreach (var table in allTableNames)
            {
                amount += _cassandraMapper.First<int>($"SELECT COUNT(*) FROM {table}");
            }
            return amount;
        }

        public void CreateCollectionIfNotExists<M>(ICreateCollectionModel<M> createCollectionModel) where M : IModel, new()
        {
            if (this.GetAllTableNames().Contains(typeof(M).Name.ToLower()) == false)
            {
                var createCollectionTable = createCollectionModel.GetCreateCollectionText();
                _cassandraMapper.Execute(createCollectionTable);
            }
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

        public void TruncateAll()
        {
            var allTableNamesInKeyspace = this.GetAllTableNames();
            foreach (var table in allTableNamesInKeyspace)
            {
                _cassandraMapper.Execute($"TRUNCATE {this.KEYSPACE_NAME}.{table}");
            }
        }

        public void Truncate<M>() where M : IModel, new()
        {
            _cassandraMapper.Execute($"TRUNCATE {this.KEYSPACE_NAME}.{typeof(M).Name}");
        }

        public IEnumerable<string> GetAllTableNames()
        {
            var query = $"SELECT table_name FROM system_schema.tables WHERE keyspace_name = '{this.KEYSPACE_NAME}';";
            return _cassandraMapper.Fetch<string>(query);
        }
    }
}
