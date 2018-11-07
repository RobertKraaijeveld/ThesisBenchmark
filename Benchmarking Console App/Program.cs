using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Benchmarking_Console_App.Configurations.Databases;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var connectionStringFactory = new DatabaseConnectionStringFactory();


            //var databaseApi = new SimplePerstDatabaseApi("benchmarkdb.dbs");
            //var dummyCreateModel = new SqlCreateModel();
            //DatabaseSeeder.SeedRandomly<SensorValueModel>(databaseApi, dummyCreateModel, amount: 1000);
            //var allModels = databaseApi.Get<SensorValueModel>("SensorValue", new SqlSearchModel());



            //var databaseApi = new SimpleMongoDbDatabaseApi(connectionStringFactory.GetDatabaseConnectionString(EDatabaseType.MongoDB));

            //DatabaseSeeder.SeedRandomly<SensorValueModel>(databaseApi, new MongoDbCreateModel(), amount: 1000);

            //var stuffToFilterOn = new Dictionary<string, object>();
            //var stuffToSelect = typeof(SensorValueModel).GetFields(BindingFlags.Instance | BindingFlags.Public)
            //                                            .Select(x => x.Name)
            //                                            .ToList();
            //var allModels = databaseApi.Get<SensorValueModel>("SensorValue", new MongoDbSearchModel(stuffToFilterOn, stuffToSelect));


            var cassandraConnString = connectionStringFactory.GetDatabaseConnectionString(EDatabaseType.Cassandra);
            var databaseApi = new SimpleCassandraDatabaseApi(cassandraConnString);

            //DatabaseSeeder.SeedRandomly<SensorValueModel>(databaseApi, new CassandraCreateModel(), 1000);
            var stuffToSelect = typeof(SensorValueModel).GetFields(BindingFlags.Instance | BindingFlags.Public)
                                                        .Select(x => x.Name)
                                                        .ToList();
        }
    }
}
