using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class RedisUpdateModel : IUpdateModel
    {
        private readonly Dictionary<string, object> identifiersAndValuesToFilterOn;

        public RedisUpdateModel()
        {
            this.identifiersAndValuesToFilterOn = new Dictionary<string, object>();
        }

        public string GetUpdateString(IModel newModel)
        {
            // Update == Create in Redis :D
            var createModel = new RedisCreateModel();
            return createModel.GetCreateString(newModel);
        }

        public Dictionary<string, object> GetIdentifiersAndValuesToFilterOn()
        {
            return this.identifiersAndValuesToFilterOn;
        }
    }
}
