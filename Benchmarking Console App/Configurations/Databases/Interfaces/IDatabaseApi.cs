using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis
{
    public interface IDatabaseApi
    {
        IEnumerable<M> Get<M>(string collectionName, ISearchModel searchModel) where M : IModel, new();

        void Create<M>(IEnumerable<M> newModels, ICreateModel createModel) where M : IModel, new();

        void Update<M>(IEnumerable<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new();

        void Delete<M>(IEnumerable<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new();
    } 
}
