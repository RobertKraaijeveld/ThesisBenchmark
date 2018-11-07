using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;
using Newtonsoft.Json;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class RedisCreateModel : ICreateModel
    {
        public string GetCreateString(IModel model)
        {
            var key = model.GetPrimaryKeyPropertyValue();
            var value = JsonConvert.SerializeObject(model);

            return "SET " + $"\"{key}\" \"{value}\"";
        }
    }
}
