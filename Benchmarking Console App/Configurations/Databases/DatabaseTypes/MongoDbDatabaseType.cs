using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.MongoDB;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.Redis;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Models.DatabaseModels;
using MySql.Data.MySqlClient;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseTypes
{
    public class MongoDbDatabaseType : IDatabaseType
    {
        public EDatabaseType ToEnum()
        {
            return EDatabaseType.MongoDB;
        }

        public CrudModels<M> GetCrudModelsForDatabaseType<M>() where M : IModel, new()
        {
            return new CrudModels<M>()
            {
                CreateModel = new MongoDbCreateModel(),
                DeleteModel = new MongoDbDeleteModel(),
                GetAllModel = new MongoDbGetAllModel<M>(),
                SearchModel = new MongoDbSearchModel<M>(),
                UpdateModel = new MongoDbUpdateModel()
            };
        }

        public DatabaseApis GetDatabaseApi()
        {
            var connString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.MongoDB);

            return new DatabaseApis()
            {
                NormalDatabaseApi = new SimpleMongoDbDatabaseApi(connString),
                ObjectOrientedDatabaseApi = null
            };
        }
    }
}
