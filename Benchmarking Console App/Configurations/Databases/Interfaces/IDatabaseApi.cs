using System.Collections.Generic;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis
{
    public interface IDatabaseApi
    {
        void OpenConnection();
        void CloseConnection();


        List<M> Search<M>(List<ISearchModel<M>> searchModels) where M : IModel, new();

        int Amount<M>() where M : IModel, new();


        void Create<M>(List<M> newModels, ICreateModel createModel) where M : IModel, new();

        void Update<M>(List<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new();

        void Delete<M>(List<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new();

        void Truncate<M>() where M : IModel, new();
    }
}
