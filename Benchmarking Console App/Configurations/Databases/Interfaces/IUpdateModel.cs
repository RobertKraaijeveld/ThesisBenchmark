using System.Collections.Generic;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.Interfaces
{
    public interface IUpdateModel
    {
        string GetUpdateString(IModel newModel);

        string[] IdentifiersToFilterOn { get; set; }
    }
}
