using System;
using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.Interfaces
{
    public interface IObjectOrientedDatabaseApi
    {
        IEnumerable<M> GetAll<M>() where M : IModel, new();

        IEnumerable<M> GetByComparison<M>(M patternObject) where M : IModel, new();

        IEnumerable<M> GetByRange<M>(M low, M high) where M : IModel, new();

        int Amount<M>() where M : IModel, new();

        void Create<M>(IEnumerable<M> newModels) where M : IModel, new();

        void Update<M>(IEnumerable<M> modelsWithNewValues) where M : IModel, new();

        void Delete<M>(IEnumerable<M> modelsToDelete) where M : IModel, new();

        void DeleteAll();
    }
}
