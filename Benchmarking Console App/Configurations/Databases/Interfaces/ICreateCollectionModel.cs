using System;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.Interfaces
{
    public interface ICreateCollectionModel<M> where M : IModel, new()
    {
        String GetCreateCollectionText();
    }
}
