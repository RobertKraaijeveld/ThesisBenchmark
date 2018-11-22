using Benchmarking_Console_App.Configurations.Databases.DatabaseApis;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.Cassandra;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseTypes
{
    public class CassandraDatabaseType : IDatabaseType
    {
        public EDatabaseType ToEnum()
        {
            return EDatabaseType.Cassandra;
        }

        public CrudModels<M> GetCrudModelsForDatabaseType<M>() where M : IModel, new()
        {
            return new CrudModels<M>()
            {
                CreateModel = new CassandraCreateModel(),
                DeleteModel = new CassandraDeleteModel(),
                SearchModel = new CassandraSearchModel<M>(),
                UpdateModel = new CassandraUpdateModel()
            };
        }

        public DatabaseApis GetDatabaseApis()
        {
            var connString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.Cassandra);

            return new DatabaseApis()
            {
                NormalDatabaseApi = new SimpleCassandraDatabaseApi(connString),
                ObjectOrientedDatabaseApi = null
            };
        }
    }
}
