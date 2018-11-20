using Benchmarking_Console_App.Configurations.Databases.DatabaseApis;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Models.DatabaseModels;
using MySql.Data.MySqlClient;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseTypes
{
    public class MySqlWithDapperDatabaseType : IDatabaseType
    {
        public EDatabaseType ToEnum()
        {
            return EDatabaseType.MySQLWithDapper;
        }

        public string GetName()
        {
            return "Dapper MySQL";
        }

        public CrudModels<M> GetCrudModelsForDatabaseType<M>() where M : IModel, new()
        {
            return new CrudModels<M>()
            {
                CreateModel = new SqlCreateModel(),
                DeleteModel = new SqlDeleteModel(),
                GetAllModel = new SqlGetAllModel<M>(),
                SearchModel = new SqlSearchModel<M>(),
                UpdateModel = new SqlUpdateModel()
            };
        }

        public DatabaseApis GetDatabaseApis()
        {
            // uses the same database as normal MySQL, so connection string uses EDatabaseType.MySQL
            var connString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.MySQL); 

            return new DatabaseApis()
            {
                NormalDatabaseApi = new SimpleDapperOrmDatabaseApi<MySqlConnection>(connString),
                ObjectOrientedDatabaseApi = null
            };
        }
    }
}
