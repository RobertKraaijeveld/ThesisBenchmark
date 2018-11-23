using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
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
        private bool _connectionIsClustered;
        private readonly string _connectionString;
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _databaseConnection;


        public SimpleRedisDatabaseApi(string connectionString)
        {
            _connectionString = connectionString;

            _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
        }


        public void OpenConnection()
        {
            _databaseConnection = _connectionMultiplexer.GetDatabase();
        }

        public void CloseConnection()
        {
            _databaseConnection = null;
        }


        public List<M> Search<M>(List<ISearchModel<M>> searchModels) where M : IModel, new()
        {
            var allSearchKeys = searchModels.Select(x => (RedisKey) x.GetSearchString<M>())
                                                          .ToList();

            var taskList = new List<Task<RedisValue>>();
            var readBatch = _databaseConnection.CreateBatch();

            foreach (var key in allSearchKeys)
            {
                var task = readBatch.StringGetAsync(key);
                taskList.Add(task);
            }
            readBatch.Execute();

            return taskList.Select(x => JsonConvert.DeserializeObject<M>(x.Result)).ToList();
        }


        public int Amount<M>() where M : IModel, new()
        {
            // returns count of all keys. We cannot get the amount of keys for a specific type
            // without doing an O(N) loop, so this less specific solution is used instead.
            return (int)_databaseConnection.Execute("DBSIZE");
        }


        public void Create<M>(List<M> newModels, ICreateModel createModel) where M : IModel, new()
        {
            List<Task> creationTasks = new List<Task>();

            foreach (var model in newModels)
            {
                var createStr = createModel.GetCreateString(model);
                var cmdAndArgs = this.SeparateCmdAndArguments(createStr);

                var createModelTask = _databaseConnection.ExecuteAsync(cmdAndArgs.Item1, cmdAndArgs.Item2);
                creationTasks.Add(createModelTask);
            }

            Task.WaitAll(creationTasks.ToArray());
        }

        public void Update<M>(List<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new()
        {
            List<Task> updatingTasks = new List<Task>();

            foreach (var model in modelsWithNewValues)
            {
                var updateStr = updateModel.GetUpdateString(model);
                var cmdAndArgs = this.SeparateCmdAndArguments(updateStr);

                var updateModelTask = _databaseConnection.ExecuteAsync(cmdAndArgs.Item1, cmdAndArgs.Item2);
                updatingTasks.Add(updateModelTask);
            }

            Task.WaitAll(updatingTasks.ToArray());
        }

        public void Delete<M>(List<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new()
        {
            List<Task> deletionTasks = new List<Task>();

            foreach (var model in modelsToDelete)
            {
                var deleteStr = deleteModel.GetDeleteString(model);
                var cmdAndArgs = this.SeparateCmdAndArguments(deleteStr);
                
                // Note that this ExecuteAsync command will only be executed once creationBatch.Execute() is called
                var deleteTask = _databaseConnection.ExecuteAsync(cmdAndArgs.Item1, cmdAndArgs.Item2);
                deletionTasks.Add(deleteTask);
            }

            Task.WaitAll(deletionTasks.ToArray());
        }

        public void TruncateAll()
        {
            using (var adminConnection = ConnectionMultiplexer.Connect($"{_connectionString},allowAdmin=true"))
            {
                var endpoints = adminConnection.GetEndPoints();
                var masterServers = endpoints.Select(x => adminConnection.GetServer(x))
                                             .Where(x => !x.IsSlave)
                                             .ToList();
                masterServers.ForEach(x => x.FlushDatabase());
            }
        }

        public void Truncate<M>() where M : IModel, new()
        {
            this.TruncateAll(); // Dangerous behavior, need to fix this
        }



        private Tuple<string, string[]> SeparateCmdAndArguments(string cmdString)
        {
            var splitCmd = cmdString.Split(' ');
            return new Tuple<string, string[]>(splitCmd[0], splitCmd.Skip(1).ToArray());
        }

        private List<M> SerializeRedisValues<M>(RedisValue[] values) where M : IModel, new()
        {
            var results = new List<M>();

            // We can't tell whether a value is of type M beforehand. So we just try
            // and try serialize the value; if this fails, its not of type M and we just continue.
            foreach (var value in values)
            {
                try
                {
                    var valueStr = value.ToString();

                    // stripping away first enclosing single quotes, otherwise JSON deserialization fails 
                    var valueWithoutEnclosingQuotes = valueStr.Remove(0, 1);
                    valueWithoutEnclosingQuotes = valueWithoutEnclosingQuotes.Remove(valueWithoutEnclosingQuotes.Length - 1, 1);

                    results.Add(JsonConvert.DeserializeObject<M>(valueWithoutEnclosingQuotes));
                }
                catch (JsonSerializationException e)
                { } // Do nothing and continue
                catch (Exception e)
                {
                    throw e; // Other type of exception, so it needs to be thrown
                }
            }

            return results;
        }
    }
}
