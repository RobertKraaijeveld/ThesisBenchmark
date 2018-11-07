using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis.Redis
{
    public class RedisSearchModel : ISearchModel
    {
        public string KeyToRetrieve { get; set; }

        public RedisSearchModel(string keyToRetrieve)
        {
            this.KeyToRetrieve = keyToRetrieve;
        }

        // TODO: Ugly param solution
        public string GetSearchString(string tableName = "not needed, redis is a kv store")
        {
            // Redis C# API has GET method so no further command text is necessary here.
            return $"\"{KeyToRetrieve}\"";
        }
    }
}
