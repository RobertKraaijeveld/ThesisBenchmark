using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Benchmarking_Console_App.Configurations.Databases;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.Cassandra;
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

        private readonly int BATCH_SIZE = 10;
        private readonly string _connectionString;

        private readonly Cluster _cassandraCluster;
        private readonly ISession _cassandraSession;
        private readonly IMapper _cassandraMapper;


        public SimpleCassandraDatabaseApi(string connectionString)
        {
            _connectionString = connectionString;

            var consistencyLevel = ConsistencyLevel.One;
            var queryOptions = new QueryOptions();
            queryOptions.SetConsistencyLevel(consistencyLevel);

            var builder = Cluster.Builder()
                                 .WithConnectionString(_connectionString)
                                 .WithQueryOptions(queryOptions);

            _cassandraCluster = builder.Build();
            _cassandraSession = _cassandraCluster.Connect(KEYSPACE_NAME);
            _cassandraMapper = new Mapper(_cassandraSession);
        }


        public void OpenConnection()
        {
        }

        public void CloseConnection()
        {
        }


        public List<M> Search<M>(List<ISearchModel<M>> searchModels) where M : IModel, new()
        {
            var searchQueries = searchModels.Select(x => x.GetSearchString<M>())
                                            .ToList();

            var executeQueriesAsyncTask = Task.WhenAll(searchQueries.Select(q => _cassandraMapper.FetchAsync<M>(q)));
            executeQueriesAsyncTask.Wait();

            return executeQueriesAsyncTask.Result.SelectMany(x => x).ToList();
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

        public IEnumerable<string> GetAllTableNames()
        {
            var query = $"SELECT table_name FROM system_schema.tables WHERE keyspace_name = '{this.KEYSPACE_NAME}';";
            return _cassandraMapper.Fetch<string>(query);
        }


        public void CreateCollectionIfNotExists<M>(ICreateCollectionModel<M> createCollectionModel) where M : IModel, new()
        {
            if (this.GetAllTableNames().Contains(typeof(M).Name.ToLower()) == false)
            {
                var createCollectionTable = createCollectionModel.GetCreateCollectionText();
                _cassandraMapper.Execute(createCollectionTable);
            }
        }

        public void Create<M>(List<M> newModels, ICreateModel createModel) where M : IModel, new()
        {
            var queries = newModels.Select(m => createModel.GetCreateString(m)).ToList();
            var batches = CreateBatches(queries);

            batches.ForEach(b => _cassandraSession.ExecuteAsync(b));
        }

        public void Update<M>(List<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new()
        {
            var queries = modelsWithNewValues.Select(m => updateModel.GetUpdateString(m)).ToList();
            var batches = CreateBatches(queries);

            batches.ForEach(b => _cassandraSession.ExecuteAsync(b));
        }

        public void Delete<M>(List<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new()
        {
            var queries = modelsToDelete.Select(m => deleteModel.GetDeleteString(m)).ToList();
            var batches = CreateBatches(queries);

            batches.ForEach(b => _cassandraSession.ExecuteAsync(b));
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


        private List<BatchStatement> CreateBatches(List<string> cqlQueries)
        {
            var batches = new List<BatchStatement>();

            if (cqlQueries.Count >= BATCH_SIZE) // Need to create more than 1 batch
            {
                for (int i = 0; i < cqlQueries.Count; i += BATCH_SIZE)
                {
                    var currBatchQueries = cqlQueries.GetRange(i, BATCH_SIZE);
                    var batch = CreateSingleBatch(currBatchQueries);

                    batches.Add(batch);
                }
            }
            else // Total amount of queries is < batch size so only 1 batch needed
            {
                var batch = CreateSingleBatch(cqlQueries);
                batches.Add(batch);
            }
            return batches;
        }

        private BatchStatement CreateSingleBatch(List<string> cqlQueries)
        {
            var batch = new BatchStatement();
            batch.SetBatchType(BatchType.Logged);

            cqlQueries.ForEach(q => batch.Add(new SimpleStatement(q)));

            return batch;
        }

    }

}
