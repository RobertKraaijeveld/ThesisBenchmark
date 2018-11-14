using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.Cassandra;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.MongoDB;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_Console_App.Configurations.Databases.DatabaseTypes;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_Console_App.Configurations.ORMs.EntityFramework;
using Benchmarking_Console_App.Models.DatabaseModels;
using Benchmarking_Console_App.Testing;
using Benchmarking_Console_App.Tests.TestReport;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Models.DatabaseModels;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Npgsql;

namespace Benchmarking_program
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var hasScalingBeenEnabled = DatabaseConnectionStringFactory.IsConfigFileForScaledServersUsed();
            RunSimpleDriversTests(hasScalingBeenEnabled);
        }

        private static void RunSimpleDriversTests(bool hasScalingBeenEnabled)
        {
            //CreateSqlCollections();

            //List<TestReport> allSimpleDriverTestReports = new List<TestReport>();
            //int[] modelAmounts = new int[] { 1, 10, 100, 500, 1000, 5000 };

            //foreach (var amount in modelAmounts)
            //{
            //    var simpleDriverTest = new DbWithSimpleDriverTest(amount, amount, amount, amount);

            //    var perstTestReport = simpleDriverTest.Test<MinuteAveragesRowForPerst>(databaseType: new PerstDatabaseType());
            //    var mySqlTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new MySQLDatabaseType());
            //    var postgreSqlTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new PostgreSQLDatabaseType());
            //    var redisTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new RedisDatabaseType());
            //    var cassandraTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new CassandraDatabaseType());
            //    var mongoTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new MongoDbDatabaseType());

            //    allSimpleDriverTestReports.Add(mySqlTestReport);
            //    allSimpleDriverTestReports.Add(postgreSqlTestReport);
            //    allSimpleDriverTestReports.Add(cassandraTestReport);
            //    allSimpleDriverTestReports.Add(mongoTestReport);
            //    allSimpleDriverTestReports.Add(redisTestReport);
            //    allSimpleDriverTestReports.Add(perstTestReport);
            //}

            //string reportName = hasScalingBeenEnabled ? "scaled_simple_drivers_tests" : "unscaled_simple_drivers_tests";
            //TestReport.CombineTestReportsIntoCsvFile(allSimpleDriverTestReports, reportName);

            // TODO: FIXME
            var pathToVisualisationScript = TestReport.GetPathToCsvOutputs() + "\\visualize_test_report.R";
            RScriptRunner.RunFromCmd(pathToVisualisationScript, "C:\\Program Files\\R\\R-3.5.1\\bin\\Rscript.exe", TestReport.GetPathToCsvOutputs().Replace('\\', '/'));
        }

        private static void CreateSqlCollections()
        {
            // Creating collections in case they dont exist yet. Only applies to fixed-schema, SQL-like databases.
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


        private static void TestSqlDatabasesUsingHeavyOrm()
        {
            // Still WIP testing work, to be made into full tests.
            var postgresqlConnString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.PostgreSQL);
            using (var benchmarkContext = new BenchmarkDbContext(postgresqlConnString))
            {
                var minuteAverageRows = benchmarkContext.MinuteAveragesRows.Take(10);
                Console.Read();
            }
        }
    }
}
