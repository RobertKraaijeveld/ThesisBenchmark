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
            var hasScalingBeenEnabled = DatabaseConnectionStringFactory.IsConfigFileForScaledServersUsed();

            RunCqrsTests(hasScalingBeenEnabled, wipeExistingDatabase: true);
            RunSimpleDriversTests(hasScalingBeenEnabled, wipeExistingDatabase: true);
        }

        // TODO: Create distinction through params between simple api, oo api and orm api tests
        private static void RunSimpleDriversTests(bool hasScalingBeenEnabled, bool wipeExistingDatabase)
        {
            CreateSqlCollections();

            List<TestReport> allTestReports = new List<TestReport>();
            int[] modelAmounts = new[] { 100, 250, 500, 1000, 5000, 10000, 50000 };

            foreach (var amount in modelAmounts)
            {
                var simpleDriverTest = new DbWithSimpleDriverTest(amount, amount / 2, amount / 2, amount / 2);

                ////Simple driver tests
                //var perstSimpleDriverTestReport = simpleDriverTest.GetTestReport<MinuteAveragesRowForPerst>(databaseType: new PerstDatabaseType(),
                //                                                                                            scaled: hasScalingBeenEnabled,
                //                                                                                            wipeExistingDatabase: wipeExistingDatabase);

                //var mySqlSimpleDriverTestReport = simpleDriverTest.GetTestReport<MinuteAveragesRow>(databaseType: new MySQLDatabaseType(),
                //                                                                                    scaled: hasScalingBeenEnabled, 
                //                                                                                    wipeExistingDatabase: wipeExistingDatabase);

                //var postgreSqlSimpleDriverTestReport = simpleDriverTest.GetTestReport<MinuteAveragesRow>(databaseType: new PostgreSQLDatabaseType(),
                //                                                                                         scaled: hasScalingBeenEnabled,
                //                                                                                         wipeExistingDatabase: wipeExistingDatabase);

                var redisSimpleDriverTestReport = simpleDriverTest.GetTestReport<MinuteAveragesRow>(databaseType: new RedisDatabaseType(),
                                                                                                    scaled: hasScalingBeenEnabled,
                                                                                                    wipeExistingDatabase: wipeExistingDatabase);

                var cassandraSimpleDriverTestReport = simpleDriverTest.GetTestReport<MinuteAveragesRow>(databaseType: new CassandraDatabaseType(),
                                                                                                        scaled: hasScalingBeenEnabled,
                                                                                                        wipeExistingDatabase: wipeExistingDatabase);

                //var mongoSimpleDriverTestReport = simpleDriverTest.GetTestReport<MinuteAveragesRow>(databaseType: new MongoDbDatabaseType(),
                //                                                                                    scaled: hasScalingBeenEnabled,
                //                                                                                    wipeExistingDatabase: wipeExistingDatabase);

                //// Dapper is technically an ORM, but it's such a thin layer over standard ADO.net that we decided 
                //// to re-use the simpleDriverTest for it. The SqlDapperOrmDatabaseApi implements IDatabaseApi anyway so it's okay.
                //var mySqlDapperTestReport = simpleDriverTest.GetTestReport<MinuteAveragesRow>(databaseType: new MySqlWithDapperDatabaseType(),
                //                                                                              scaled: hasScalingBeenEnabled, 
                //                                                                              wipeExistingDatabase: wipeExistingDatabase);

                ////allTestReports.Add(mySqlSimpleDriverTestReport);
                //allTestReports.Add(postgreSqlSimpleDriverTestReport);
                allTestReports.Add(cassandraSimpleDriverTestReport);
                //allTestReports.Add(mongoSimpleDriverTestReport);
                allTestReports.Add(redisSimpleDriverTestReport);
                //allTestReports.Add(perstSimpleDriverTestReport);
                //allTestReports.Add(mySqlDapperTestReport);

                //Entity framework tests
                var entityFrameworkTest = new DbWithEntityFrameworkTest(amount, amount / 2, amount / 2, amount / 2);

                var mysqlOrmTestReport = entityFrameworkTest.GetTestReport<MinuteAveragesRow>(databaseType: new MySQLDatabaseType(),
                                                                                              scaled: hasScalingBeenEnabled,
                                                                                              wipeExistingDatabase: wipeExistingDatabase);
                //var postgreSqlOrmTestReport = entityFrameworkTest.GetTestReport<MinuteAveragesRow>(databaseType: new PostgreSQLDatabaseType(),
                //                                                                                   scaled: hasScalingBeenEnabled);

                allTestReports.Add(mysqlOrmTestReport);
                //allTestReports.Add(postgreSqlOrmTestReport);

                Console.WriteLine($"Done with test for {amount} models...");
            }

            string reportName = hasScalingBeenEnabled ? "scaled_simple_drivers_tests" : "unscaled_simple_drivers_tests";
            TestReport.CombineTestReportsIntoCsvFile(allTestReports, reportName);
        }

        // TODO: Duplication here.
        private static void RunCqrsTests(bool hasScalingBeenEnabled, bool wipeExistingDatabase)
        {
            CreateSqlCollections();

            List<TestReport> allTestReports = new List<TestReport>();
            int[] modelAmounts = new[] {100, 250, 500, 1000, 5000, 10000, 50000};

            foreach (var amount in modelAmounts)
            {
                var cqrsTest = new DbWithCqrsTest(amount, amount, amount, amount, readDatabaseType: new RedisDatabaseType());
                var cqrsSqlAndRedisTestReport = cqrsTest.GetTestReport<MinuteAveragesRow>(new MySQLDatabaseType(), 
                                                                                          scaled: hasScalingBeenEnabled,
                                                                                          wipeExistingDatabase: wipeExistingDatabase);
                allTestReports.Add(cqrsSqlAndRedisTestReport);
            }

            string reportName = hasScalingBeenEnabled ? "scaled_cqrs_tests" : "unscaled_cqrs_tests";
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
    }
}
