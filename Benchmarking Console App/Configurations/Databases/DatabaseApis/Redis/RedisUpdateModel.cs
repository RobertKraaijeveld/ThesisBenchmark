using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class RedisUpdateModel : IUpdateModel
    {
        // Models primary key is used to filter on, so this array is not used here...
        public string[] IdentifiersToFilterOn { get; set; }

        public string GetUpdateString(IModel newModel)
        {
            // Update == Create in Redis :D
            var createModel = new RedisCreateModel();
            return createModel.GetCreateString(newModel);
        }
    }
}
