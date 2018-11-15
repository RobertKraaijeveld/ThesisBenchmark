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
using Benchmarking_Console_App.Tests;
using Benchmarking_Console_App.Tests.ORM;
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
            CreateSqlCollections();

            List<TestReport> allTestReports = new List<TestReport>();
            int[] modelAmounts = new int[] { 500 };

            foreach (var amount in modelAmounts)
            {
                var simpleDriverTest = new DbWithSimpleDriverTest(amount, amount, amount, amount);

                // Simple driver tests
                var perstSimpleDriverTestReport = simpleDriverTest.Test<MinuteAveragesRowForPerst>(databaseType: new PerstDatabaseType(), scaled: hasScalingBeenEnabled);
                var mySqlSimpleDriverTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new MySQLDatabaseType(), scaled: hasScalingBeenEnabled);
                var postgreSqlSimpleDriverTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new PostgreSQLDatabaseType(), scaled: hasScalingBeenEnabled);
                var redisSimpleDriverTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new RedisDatabaseType(), scaled: hasScalingBeenEnabled);
                var cassandraSimpleDriverTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new CassandraDatabaseType(), scaled: hasScalingBeenEnabled);
                var mongoSimpleDriverTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new MongoDbDatabaseType(), scaled: hasScalingBeenEnabled);

                allTestReports.Add(mySqlSimpleDriverTestReport);
                allTestReports.Add(postgreSqlSimpleDriverTestReport);
                allTestReports.Add(cassandraSimpleDriverTestReport);
                allTestReports.Add(mongoSimpleDriverTestReport);
                allTestReports.Add(redisSimpleDriverTestReport);
                allTestReports.Add(perstSimpleDriverTestReport);

                // ORM tests
                var ormTest = new DbWithOrmTest(amount, amount, amount, amount);

                var mysqlOrmTestReport = ormTest.Test<MinuteAveragesRow>(databaseType: new MySQLDatabaseType(), scaled: hasScalingBeenEnabled);
                var postgreSqlOrmTestReport = ormTest.Test<MinuteAveragesRow>(databaseType: new PostgreSQLDatabaseType(), scaled: hasScalingBeenEnabled);

                allTestReports.Add(mySqlSimpleDriverTestReport);
                allTestReports.Add(postgreSqlOrmTestReport);
            }

            string reportName = hasScalingBeenEnabled ? "scaled_simple_drivers_tests" : "unscaled_simple_drivers_tests";
            TestReport.CombineTestReportsIntoCsvFile(allTestReports, reportName);
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
