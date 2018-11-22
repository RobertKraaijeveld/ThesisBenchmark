using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis
{
    public struct CrudModels<M> where M : IModel, new()
    {
        public ICreateModel CreateModel;
        public ISearchModel<M> SearchModel;
        public IUpdateModel UpdateModel;
        public IDeleteModel DeleteModel;
    }
}
