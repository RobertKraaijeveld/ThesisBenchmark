using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis.Redis
{
    public class RedisSearchModel<M> : ISearchModel<M> where M : IModel, new()
    {
        public string KeyToRetrieve { get; set; }

        public RedisSearchModel(string keyToRetrieve)
        {
            this.KeyToRetrieve = keyToRetrieve;
        }

        public string GetSearchString<M>()
        {
            // Redis C# API has GET method so no further command text is necessary here.
            return $"\"{KeyToRetrieve}\"";
        }
    }
}
