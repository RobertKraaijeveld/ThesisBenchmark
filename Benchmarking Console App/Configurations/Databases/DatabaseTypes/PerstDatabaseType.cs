using Benchmarking_Console_App.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseTypes
{
    public class PerstDatabaseType : IDatabaseType
    {
        public EDatabaseType ToEnum()
        {
            return EDatabaseType.Perst;
        }

        public CrudModels<M> GetCrudModelsForDatabaseType<M>() where M : IModel, new()
        {
            // Perst has no need for your puny CRUD models, for it is an Object-Oriented DB: Tremble, mortals!
            return new CrudModels<M>();
        }

        public DatabaseApis GetDatabaseApis()
        {
            var connString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.Perst);

            return new DatabaseApis()
            {
                NormalDatabaseApi = null,
                ObjectOrientedDatabaseApi = new SimplePerstDatabaseApi(connString)
            };
        }
    }
}
