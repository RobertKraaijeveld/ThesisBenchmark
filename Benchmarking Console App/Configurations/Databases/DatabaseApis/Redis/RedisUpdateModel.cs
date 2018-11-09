using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;
using Cassandra;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class RedisUpdateModel : IUpdateModel
    {
        public string KeyToUpdate; // TODO: INCORRECT USAGE, KEY OF MODEL == PRIMARY KEY

        public RedisUpdateModel(string KeyToUpdate)
        {
            this.KeyToUpdate = KeyToUpdate;
        }

        public string GetUpdateString(IModel newModel)
        {
            // Update == Create in Redis :D
            var createModel = new RedisCreateModel();
            return createModel.GetCreateString(newModel);
        }

        public Dictionary<string, object> GetIdentifiersAndValuesToFilterOn()
        {
            return new Dictionary<string, object>()
            {
                {KeyToUpdate, null} // TODO: Quite bad practice, fix this.
            };
        }
    }
}
