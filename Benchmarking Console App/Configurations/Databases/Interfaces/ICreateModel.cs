using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.Interfaces
{
    /// <summary>
    /// Provides an interface for classes that allows a high level structure of
    /// { K: Redis-style key, SQL-style column, MongoDB-style label etc. V: Value to insert }
    /// to be turned into the appropriate query text for that particular database.
    /// </summary>
    public interface ICreateModel
    {
        string GetCreateString(IModel model);
    }
}
