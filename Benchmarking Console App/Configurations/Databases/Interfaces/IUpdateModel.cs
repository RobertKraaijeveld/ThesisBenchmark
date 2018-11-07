using System;
using System.Collections.Generic;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.Interfaces
{
    /// <summary>
    /// Provides an interface for classes that allows a high level structure of
    /// {
    ///     {
    ///         K1: Redis-style key, SQL-style column, MongoDB-style label etc to filter on
    ///         K2: Redis-style key, SQL-style column, MongoDB-style label etc to be updated
    ///     } 
    ///     {
    ///         V1: Value to filter on
    ///         V2: Value to update to
    ///     }
    /// }
    /// to be turned into the appropriate query text for that particular database.
    /// </summary>
    public interface IUpdateModel
    {
        Dictionary<string, object> GetIdentifiersAndValuesToFilterOn();
        string GetUpdateString(IModel newModel);
    }
}
