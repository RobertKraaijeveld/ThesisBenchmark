using System;
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
            //TestSqlDatabasesUsingHeavyOrm();


            // Creating collections in case they dont exist yet
            var mysqlConnString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.MySQL);
            var mysqlApi = new SimpleSQLDatabaseApi<MySqlCommand, MySqlConnection>(mysqlConnString);

            var postgresqlConnString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.PostgreSQL);
            var postgresqlApi = new SimpleSQLDatabaseApi<NpgsqlCommand, NpgsqlConnection>(postgresqlConnString);

            mysqlApi.CreateCollectionIfNotExists(new SqlCreateCollectionModel<MinuteAveragesRow>());
            postgresqlApi.CreateCollectionIfNotExists(new SqlCreateCollectionModel<MinuteAveragesRow>());


            // Testing using only simple, hand-coded drivers
            var simpleDriverTest = new DbWithSimpleDriverTest(100, 100, 100, 100);

            var perstTestReport = simpleDriverTest.Test<MinuteAveragesRowForPerst>(databaseType: new PerstDatabaseType());
            var mySqlTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new MySQLDatabaseType());
            var postgreSQLTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new PostgreSQLDatabaseType());
            var redisTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new RedisDatabaseType());
            var cassandraTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new CassandraDatabaseType());
            var mongoTestReport = simpleDriverTest.Test<MinuteAveragesRow>(databaseType: new MongoDbDatabaseType());


            // Testing using ORM (for the DB's that support it)
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
