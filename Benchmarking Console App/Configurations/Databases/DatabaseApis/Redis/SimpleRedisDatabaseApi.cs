using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ConnectionMultiplexer _connection;

        public SimpleRedisDatabaseApi(string connectionString)
        {
            _connectionString = connectionString;

            _connection = ConnectionMultiplexer.Connect(connectionString);
        }

        public IEnumerable<M> GetAll<M>(IGetAllModel<M> getAllModel) where M : IModel, new() 
        {
            var getAllCommandText = getAllModel.CreateGetAllString();
            var cmdAndArgs = this.SeparateCmdAndArguments(getAllCommandText);

            var db = _connection.GetDatabase();
            var allKeys = (RedisKey[]) db.Execute(cmdAndArgs.Item1, cmdAndArgs.Item2);

            try
            {
                return this.SerializeRedisValues<M>(db.StringGet(allKeys));
            }
            catch (Exception e) // Clustered redis throws exceptions when trying to retrieve set of keys all at once.
            {
                // So we retrieve them one-by-one instead.
                List<RedisValue> values = new List<RedisValue>();

                foreach (var redisKey in allKeys)
                {
                    var val = db.StringGet(redisKey);
                    values.Add(val);
                }
                return this.SerializeRedisValues<M>(values.ToArray());
            }
        }

        public IEnumerable<M> Search<M>(ISearchModel<M> searchModel) where M : IModel, new()
        {
            var searchCommandText = searchModel.GetSearchString<M>();

            var db = _connection.GetDatabase();

            var resultJson = db.StringGet(searchCommandText);
            return new List<M>() { JsonConvert.DeserializeObject<M>(resultJson) };
        }

        public int Amount<M>() where M : IModel, new()
        {
            var db = _connection.GetDatabase();

            // returns count of all keys. We cannot get the amount of keys for a specific type
            // without doing an O(N) loop, so this less specific solution is used instead.
            return (int) db.Execute("DBSIZE");
        }

        public void Create<M>(IEnumerable<M> newModels, ICreateModel createModel) where M : IModel, new()
        {
            var db = _connection.GetDatabase();

            foreach (var model in newModels)
            {
                var createStr = createModel.GetCreateString(model);
                var cmdAndArgs = this.SeparateCmdAndArguments(createStr);

                db.Execute(cmdAndArgs.Item1, cmdAndArgs.Item2);
            }
        }

        public void Update<M>(IEnumerable<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new()
        {
            var db = _connection.GetDatabase();

            foreach (var model in modelsWithNewValues)
            {
                var updateStr = updateModel.GetUpdateString(model);
                var cmdAndArgs = this.SeparateCmdAndArguments(updateStr);

                db.Execute(cmdAndArgs.Item1, cmdAndArgs.Item2);
            }
        }

        public void Delete<M>(IEnumerable<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new()
        {
            var db = _connection.GetDatabase();

            foreach (var model in modelsToDelete)
            {
                var deleteStr = deleteModel.GetDeleteString(model);
                var cmdAndArgs = this.SeparateCmdAndArguments(deleteStr);

                db.Execute(cmdAndArgs.Item1, cmdAndArgs.Item2);
            }
        }

        public void TruncateAll()
        {
            // Connecting with admin rights, then flushing all databases on all cluster nodes
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

        private IEnumerable<M> SerializeRedisValues<M>(RedisValue[] values) where M: IModel, new()
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
