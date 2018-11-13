using System.Collections.Generic;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.Interfaces
{
    /// <summary>
    /// Provides an interface for classes that allows a high level structure of
    /// { K1: Redis-style key, SQL-style column, MongoDB-style label etc to filter on, V: Value to filter on }
    /// to be turned into the appropriate query text for that particular database.
    /// </summary>
    public interface IDeleteModel
    {
        string[] IdentifiersToDeleteOn { get; set; }

        string GetDeleteString(IModel model);
    }
}
