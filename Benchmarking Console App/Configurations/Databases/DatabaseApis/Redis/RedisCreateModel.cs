using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class RedisCreateModel : ICreateModel
    {
        public string GetCreateString(IModel model)
        {
            var key = model.GetFieldsWithValues()[model.GetPrimaryKeyFieldName()];
            var value = JsonConvert.SerializeObject(model);

            return "SET " + $"'{key}' '{value}'";
        }
    }
}
