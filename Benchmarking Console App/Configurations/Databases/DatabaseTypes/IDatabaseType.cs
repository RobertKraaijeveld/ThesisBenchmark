using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseTypes
{
    public interface IDatabaseType
    {
        EDatabaseType ToEnum();
        DatabaseApis GetDatabaseApi();
        CrudModels<M> GetCrudModelsForDatabaseType<M>() where M : IModel, new();
    }

    public struct DatabaseApis
    {
        public IDatabaseApi NormalDatabaseApi;
        public IObjectOrientedDatabaseApi ObjectOrientedDatabaseApi;
    }

    public struct CrudModels<M> where M : IModel, new()
    {
        public ICreateModel CreateModel;
        public IGetAllModel<M> GetAllModel;
        public ISearchModel<M> SearchModel;
        public IUpdateModel UpdateModel;
        public IDeleteModel DeleteModel;
    }
}
