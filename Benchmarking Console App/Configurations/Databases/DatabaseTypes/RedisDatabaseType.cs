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
    public class RedisDatabaseType : IDatabaseType
    {
        public EDatabaseType ToEnum()
        {
            return EDatabaseType.Redis;
        }

        public CrudModels<M> GetCrudModelsForDatabaseType<M>() where M : IModel, new()
        {
            return new CrudModels<M>()
            {
                CreateModel = new RedisCreateModel(),
                DeleteModel = new RedisDeleteModel(),
                GetAllModel = new RedisGetAllModel<M>(),
                SearchModel = new RedisSearchModel<M>(),
                UpdateModel = new RedisUpdateModel()
            };
        }

        public DatabaseApis GetDatabaseApis()
        {
            var connString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.Redis);

            return new DatabaseApis()
            {
                NormalDatabaseApi = new SimpleRedisDatabaseApi(connString),
                ObjectOrientedDatabaseApi = null
            };
        }
    }
}
