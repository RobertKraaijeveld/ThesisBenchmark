using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using Benchmarking_Console_App.Configurations.Databases;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.Cassandra;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.MongoDB;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_Console_App.Models.DatabaseModels;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;
using MySql.Data.MySqlClient;
using Npgsql;

namespace Benchmarking_program
{
    public class Program
    {
        public static void Main(string[] args)
        {
            int seedAmount = 100;
            DatabaseConnectionStringFactory connectionStringFactory = new DatabaseConnectionStringFactory();

            Type[] modelTypes = new Type[1] { typeof(MinuteAveragesRow) };

            foreach (var modelType in modelTypes) // TODO: FIX THIS W/ REFLECTION
            {
                // Perst: Object-oriented 
                var perstConnString = connectionStringFactory.GetDatabaseConnectionString(EDatabaseType.Perst);
                TestOODatabase<MinuteAveragesRow>(EDatabaseType.Perst,
                                                  new SimplePerstDatabaseApi(perstConnString),
                                                  seedAmount);

                // Redis: Key-Value store
                var redisConnString = connectionStringFactory.GetDatabaseConnectionString(EDatabaseType.Redis);
                TestDatabase(EDatabaseType.Redis,
                             new SimpleRedisDatabaseApi(redisConnString),
                             new RedisCreateModel(),
                             new RedisGetAllModel<MinuteAveragesRow>(),
                             seedAmount);


                // MongoDB: Document store
                var mongoDbConnectionString = connectionStringFactory.GetDatabaseConnectionString(EDatabaseType.MongoDB);
                TestDatabase(EDatabaseType.MongoDB,
                             new SimpleMongoDbDatabaseApi(mongoDbConnectionString),
                             new MongoDbCreateModel(),
                             new MongoDbGetAllModel<MinuteAveragesRow>(),
                             seedAmount);


                // MySQL: Relational
                var mysqlConnString = connectionStringFactory.GetDatabaseConnectionString(EDatabaseType.MySQL);
                var mysqlApi = new SimpleSQLDatabaseApi<MySqlCommand, MySqlConnection>(mysqlConnString);

                mysqlApi.CreateCollectionIfNotExists(new SqlCreateCollectionModel<MinuteAveragesRow>());
                mysqlApi.Truncate("MinuteAveragesRow"); // TODO FIX THIS USING TYPENAME

                TestDatabase(EDatabaseType.MySQL,
                             mysqlApi,
                             new SqlCreateModel(),
                             new SqlGetAllModel<MinuteAveragesRow>(),
                             seedAmount);


                // PostgreSQL: Object-relational
                var postgresqlConnString = connectionStringFactory.GetDatabaseConnectionString(EDatabaseType.PostgreSQL);
                var postgreSQLApi = new SimpleSQLDatabaseApi<NpgsqlCommand, NpgsqlConnection>(postgresqlConnString);

                postgreSQLApi.CreateCollectionIfNotExists(new SqlCreateCollectionModel<MinuteAveragesRow>());
                postgreSQLApi.Truncate("MinuteAveragesRow"); // TODO FIX THIS USING TYPENAME

                TestDatabase(EDatabaseType.PostgreSQL,
                             postgreSQLApi,
                             new SqlCreateModel(),
                             new SqlGetAllModel<MinuteAveragesRow>(),
                             seedAmount);

                
                // Cassandra: Column store
                var cassandraConnString = connectionStringFactory.GetDatabaseConnectionString(EDatabaseType.Cassandra);
                var cassandraApi = new SimpleCassandraDatabaseApi(cassandraConnString);

                cassandraApi.CreateCollectionIfNotExists(new CassandraCreateCollectionModel<MinuteAveragesRow>(cassandraApi.KEYSPACE_NAME));
                cassandraApi.TruncateAll();

                TestDatabase(EDatabaseType.Cassandra,
                             cassandraApi,
                             new CassandraCreateModel(),
                             new CassandraGetAllModel<MinuteAveragesRow>(),
                             seedAmount);
            }

            Console.Read();
        }


        private static void TestDatabase<M>(EDatabaseType databaseType,
                                            IDatabaseApi api, 
                                            ICreateModel createModel, 
                                            IGetAllModel<M> getAllModel,
                                            int seedAmount) where M: IModel, new()
        {
            api.TruncateAll();

            var randomModels = GetRandomModels<M>(seedAmount);

            var creationTimeMs = GetElapsedTimeMs(() => api.Create(randomModels, createModel));
            var readAllTimeMs = GetElapsedTimeMs(() => api.GetAll(getAllModel));

            WriteResults(databaseType, typeof(M), creationTimeMs, readAllTimeMs, seedAmount);
            RunGC();
        }

        private static void TestOODatabase<M>(EDatabaseType databaseType,
                                              IObjectOrientedDatabaseApi api,
                                              int seedAmount) where M : IModel, new()
        {
            api.DeleteAll();

            var randomModels = GetRandomModels<M>(seedAmount);

            // Seeding: Creation model for query text generation is not needed
            // because we're using an Object-Oriented DB.
            var creationTimeMs = GetElapsedTimeMs(() => api.Create(randomModels));
            var readAllTimeMs = GetElapsedTimeMs(() => api.GetAll<M>());

            WriteResults(databaseType, typeof(M), creationTimeMs, readAllTimeMs, seedAmount);
            RunGC();
        }

        private static IEnumerable<M> GetRandomModels<M>(int amount) where M : IModel, new()
        {
            var random = new Random();
            var listOfNewModels = new M[amount];
            for (int i = 0; i < amount; i++)
            {
                var newModel = new M();
                newModel.Randomize(i, random);

                listOfNewModels[i] = newModel;
            }
            return listOfNewModels;
        }

        private static long GetElapsedTimeMs(Action actionToExecute)
        {
            var sw = new Stopwatch();
            sw.Start();
            actionToExecute.Invoke();
            sw.Stop();

            return sw.ElapsedMilliseconds;
        }

        private static void WriteResults(EDatabaseType databaseType, Type modelType, 
                                         long writeAllMs, long readAllMs, int amount)
        {
            Console.WriteLine($"\n");
            Console.WriteLine($"Results for {databaseType}:");
            Console.WriteLine($"-----------------------------------------------------------");
            Console.WriteLine($"Write time for {amount} models of type {modelType.Name}: {writeAllMs} ms");
            Console.WriteLine($"Read time for {amount} models of type {modelType.Name}: {readAllMs} ms");
            Console.WriteLine($"-----------------------------------------------------------");
            Console.WriteLine("\n");
        }

        private static void RunGC()
        {
            // Forcing garbage collection to ensure old models are not retained in memory.
            // This would skew the performance measurements.
            GC.Collect();
        }
    }
}
