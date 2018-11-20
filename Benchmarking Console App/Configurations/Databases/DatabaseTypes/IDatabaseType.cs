using Benchmarking_Console_App.Configurations.Databases.DatabaseApis;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseTypes
{
    public interface IDatabaseType
    {
        EDatabaseType ToEnum();
        DatabaseApis GetDatabaseApis();
        CrudModels<M> GetCrudModelsForDatabaseType<M>() where M : IModel, new();
    }

    public struct DatabaseApis
    {
        public IDatabaseApi NormalDatabaseApi;
        public IObjectOrientedDatabaseApi ObjectOrientedDatabaseApi;
    }
}
