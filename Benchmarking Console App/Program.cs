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
            int[] modelAmounts = new int[] { 100, 500, 1000, 5000, 10000, 100000, 1000000000 };

            var simpleDriverTestReports = GetSimpleDriversTestReports(modelAmounts, hasScalingBeenEnabled, wipeExistingDatabase);
            var entityFrameworkTestReports = GetEntityFrameworkTestReports(modelAmounts, hasScalingBeenEnabled, wipeExistingDatabase);
            var cqrsTestReports = GetCqrsTestReports(modelAmounts, hasScalingBeenEnabled, wipeExistingDatabase);


            string reportName = hasScalingBeenEnabled ? "scaled_simple_drivers_tests" : "unscaled_simple_drivers_tests";
            TestReport.CombineTestReportsIntoCsvFile(allTestReports, reportName);
        }

        private static void GetSimpleDriversTestReports(int[] modelAmounts, 
                                                        bool hasScalingBeenEnabled, 
                                                        bool wipeExistingDatabase)
        {
            CreateSqlCollections();

            List<TestReport> allTestReports = new List<TestReport>();

            var simpleDriverTest = new DbWithSimpleDriverTest(amount, amount, amount, amount);
            var tests = new List<Tuple<AbstractPerformanceTest, IDatabaseType>>()
            {
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new PerstDatabaseType()),
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new MySQLDatabaseType()),
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new PostgreSQLDatabaseType()),
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new RedisDatabaseType()),
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new CassandraDatabaseType()),
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new MongoDbDatabaseType()),
                new Tuple<AbstractPerformanceTest, IDatabaseType>(simpleDriverTest, new MySqlWithDapperDatabaseType())
            };

            return GetTestReport(tests, modelAmounts, hasScalingBeenEnabled, wipeExistingDatabase);
        }
            
        private static List<TestReport> GetEntityFrameworkTestReports(int[] modelAmounts, 
                                                                      bool hasScalingBeenEnabled, 
                                                                      bool wipeExistingDatabase)
        {
            CreateSqlCollections();

            List<TestReport> allTestReports = new List<TestReport>();

            var entityFrameworkTest = new DbWithEntityFrameworkTest(amount, amount, amount, amount);
            var tests = new List<Tuple<AbstractPerformanceTest, IDatabaseType>>()
            {
                new Tuple<AbstractPerformanceTest, IDatabaseType>(entityFrameworkTest, new MySQLDatabaseType()),
                // new Tuple<AbstractPerformanceTest, IDatabaseType>(entityFrameworkTest, new PostgreSQLDatabaseType())
            };

            return GetTestReport(tests, modelAmounts, hasScalingBeenEnabled, wipeExistingDatabase);
        }

        private static List<TestReport> GetCqrsTestReports(int[] modelAmounts, 
                                                           bool hasScalingBeenEnabled, 
                                                           bool wipeExistingDatabase)
        {
            CreateSqlCollections();

            var tests = new List<Tuple<AbstractPerformanceTest, IDatabaseType>>()
            {
                new Tuple<AbstractPerformanceTest, IDatabaseType>
                    (new DbWithCqrsTest(amount, amount, amount, amount, readDatabaseType: new RedisDatabaseType()),
                     new MySQLDatabaseType());
            };

            return GetTestReports(tests, modelAmounts, hasScalingBeenEnabled, wipeExistingDatabase);
        } 



        private static List<TestReport> GetTestReports(List<Tuple<AbstractPerformanceTest, IDatabaseType>> testsToExecute, 
                                                        int[] modelAmounts, 
                                                        bool hasScalingBeenEnabled, 
                                                        bool wipeExistingDatabase)
        {
            List<TestReport> resultingTestReports = new List<TestReport>();

            foreach (var amount in modelAmounts)
            {
                testsToExecute.ForEach(tpl => 
                {
                    var result = tpl.Item1.GetTestReport<MinuteAveragesRow>(tpl.Item2, hasScalingBeenEnabled, wipeExistingDatabase);
                    resultingTestReports.Add(result);
                });

                Console.WriteLine($"Done with test for {amount} models...");
            }

            return resultingTestReports;
        }

        private static void CreateSqlCollections()
        {
            // Creating collections in case they don't exist yet. Only applies to fixed-schema, SQL-like databases.
            var mysqlConnString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.MySQL);
            var mysqlApi = new SimpleSQLDatabaseApi<MySqlCommand, MySqlConnection>(mysqlConnString);

            var postgresqlConnString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.PostgreSQL);
            var postgresqlApi = new SimpleSQLDatabaseApi<NpgsqlCommand, NpgsqlConnection>(postgresqlConnString);

            var cassandraConnString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.Cassandra);
            var cassandraApi = new SimpleCassandraDatabaseApi(cassandraConnString);

            mysqlApi.CreateCollectionIfNotExists(new SqlCreateCollectionModel<MinuteAveragesRow>());
            postgresqlApi.CreateCollectionIfNotExists(new SqlCreateCollectionModel<MinuteAveragesRow>());
            cassandraApi.CreateCollectionIfNotExists(new CassandraCreateCollectionModel<MinuteAveragesRow>(cassandraApi.KEYSPACE_NAME));
        }
    }
}
