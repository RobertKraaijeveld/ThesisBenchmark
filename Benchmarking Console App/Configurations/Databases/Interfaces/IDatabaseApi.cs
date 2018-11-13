using System.Collections.Generic;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis
{
    public interface IDatabaseApi
    {
        IEnumerable<M> GetAll<M>(IGetAllModel<M> getAllModel) where M : IModel, new();

        IEnumerable<M> Search<M>(ISearchModel<M> searchModel) where M : IModel, new();

        int Amount<M>() where M : IModel, new();

        //void CreateCollectionIfNotExists<M>(ICreateCollectionModel<M> createCollectionModel) where M : IModel, new();

        void Create<M>(IEnumerable<M> newModels, ICreateModel createModel) where M : IModel, new();

        void Update<M>(IEnumerable<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new();

        void Delete<M>(IEnumerable<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new();

        void TruncateAll();

        void Truncate<M>() where M : IModel, new();
    }
}
