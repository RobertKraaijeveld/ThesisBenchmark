using System;
using System.Collections.Generic;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.Cassandra;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_Console_App.Configurations.Databases.DatabaseTypes;
using Benchmarking_Console_App.Models.DatabaseModels;
using Benchmarking_Console_App.Testing;
using Benchmarking_Console_App.Tests;
using Benchmarking_Console_App.Tests.CQRS;
using Benchmarking_Console_App.Tests.ORM;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Models.DatabaseModels;
using MySql.Data.MySqlClient;
using Npgsql;

namespace Benchmarking_program
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool hasScalingBeenEnabled = DatabaseConnectionStringFactory.IsConfigFileForScaledServersUsed();
            bool wipeExistingDatabase = true;
            int[] modelAmounts = new int[] { 1000 }; //0, 500, 1000, 5000, 10000, 100000, 1000000000};

            CreateSqlCollections();

            RunSimpleDriverTests(modelAmounts, hasScalingBeenEnabled, wipeExistingDatabase);
            RunEntityFrameworkTests(modelAmounts, hasScalingBeenEnabled, wipeExistingDatabase);
            RunCqrsTestReports(modelAmounts, hasScalingBeenEnabled, wipeExistingDatabase);
        }



        private static void RunSimpleDriverTests(int[] modelAmounts,
                                                        bool hasScalingBeenEnabled,
                                                        bool wipeExistingDatabase)
        {
            var simpleDriverTest = new DbWithSimpleDriverTest();
            var tests = new List<Tuple<AbstractPerformanceTest, IDatabaseType>>()
            {
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new MySQLDatabaseType()),
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new PostgreSQLDatabaseType()),
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new RedisDatabaseType()),
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new CassandraDatabaseType()),
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new MongoDbDatabaseType()),
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new MySqlWithDapperDatabaseType())
            };
            ExecuteTests<MinuteAveragesRow>(tests, modelAmounts, hasScalingBeenEnabled, wipeExistingDatabase);


            // Perst uses a different type of model (because it cannot cope with properties instead of fields)
            var perstTest = new List<Tuple<AbstractPerformanceTest, IDatabaseType>>()
            {
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new PerstDatabaseType())
            };
            ExecuteTests<MinuteAveragesRowForPerst>(perstTest, modelAmounts, hasScalingBeenEnabled, wipeExistingDatabase);
        }

        private static void RunEntityFrameworkTests(int[] modelAmounts,
                                                                      bool hasScalingBeenEnabled,
                                                                      bool wipeExistingDatabase)
        {
            var entityFrameworkTest = new DbWithEntityFrameworkTest();
            var tests = new List<Tuple<AbstractPerformanceTest, IDatabaseType>>()
            {
                new Tuple<AbstractPerformanceTest, IDatabaseType>(entityFrameworkTest, new MySQLDatabaseType()),
                // new Tuple<AbstractPerformanceTest, IDatabaseType>(entityFrameworkTest, new PostgreSQLDatabaseType())
            };

            ExecuteTests<MinuteAveragesRow>(tests, modelAmounts, hasScalingBeenEnabled, wipeExistingDatabase);
        }

        private static void RunCqrsTestReports(int[] modelAmounts,
                                                           bool hasScalingBeenEnabled,
                                                           bool wipeExistingDatabase)
        {
            // TODO: APPLY CQRS TO MULTIPLE DBs
            var test = new DbWithCqrsTest(readDatabaseType: new RedisDatabaseType());
            var databaseType = new MySQLDatabaseType();

            var tests = new List<Tuple<AbstractPerformanceTest, IDatabaseType>>
            {
                new Tuple<AbstractPerformanceTest, IDatabaseType>(test, databaseType)
            };

            ExecuteTests<MinuteAveragesRow>(tests, modelAmounts, hasScalingBeenEnabled, wipeExistingDatabase);
        }


        private static void ExecuteTests<M>(List<Tuple<AbstractPerformanceTest, IDatabaseType>> testsToExecute,
                                            int[] modelAmounts,
                                            bool hasScalingBeenEnabled,
                                            bool wipeExistingDatabase) where M : class, IModel, new()
        {
            List<TestReport> resultingTestReports = new List<TestReport>();

            foreach (var amount in modelAmounts)
            {
                testsToExecute.ForEach(tpl =>
                {
                    tpl.Item1.SetAmounts(amount, amount / 2, amount / 2, amount / 2);

                    var result = tpl.Item1.GetTestReport<M>(tpl.Item2, 
                                                            hasScalingBeenEnabled,
                                                            wipeExistingDatabase);

                    resultingTestReports.Add(result);
                });


                string reportName = hasScalingBeenEnabled ? "scaled_simple_drivers_tests" : "unscaled_simple_drivers_tests";
                Console.WriteLine($"Done with test for {amount} models, writing to {reportName}.csv");

                TestReport.CombineTestReportsIntoCsvFile(resultingTestReports, reportName);
            }
        }

        private static void CreateSqlCollections()
        {
            // Creating collections in case they don't exist yet. Only applies to fixed-schema, SQL-like databases.
            var mysqlConnString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.MySQL);
            var mysqlApi = new SimpleSQLDatabaseApi<MySqlCommand, MySqlConnection, MySqlDataAdapter>(mysqlConnString);

            var postgresqlConnString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.PostgreSQL);
            var postgresqlApi = new SimpleSQLDatabaseApi<NpgsqlCommand, NpgsqlConnection, NpgsqlDataAdapter>(postgresqlConnString);

            var cassandraConnString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.Cassandra);
            var cassandraApi = new SimpleCassandraDatabaseApi(cassandraConnString);

            mysqlApi.OpenConnection();
            mysqlApi.CreateCollectionIfNotExists(new SqlCreateCollectionModel<MinuteAveragesRow>());
            mysqlApi.CloseConnection();

            postgresqlApi.OpenConnection();
            postgresqlApi.CreateCollectionIfNotExists(new SqlCreateCollectionModel<MinuteAveragesRow>());
            postgresqlApi.CloseConnection();
            
            cassandraApi.OpenConnection();
            cassandraApi.CreateCollectionIfNotExists(new CassandraCreateCollectionModel<MinuteAveragesRow>(cassandraApi.KEYSPACE_NAME));
            cassandraApi.CloseConnection();
        }
    }
 }

