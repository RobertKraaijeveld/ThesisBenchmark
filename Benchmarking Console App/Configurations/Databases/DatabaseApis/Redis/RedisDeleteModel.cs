using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class RedisDeleteModel : IDeleteModel
    {
        public Dictionary<string, object> identifiersAndValuesToDeleteOn { get; set; }

        public RedisDeleteModel()
        {
            this.identifiersAndValuesToDeleteOn = new Dictionary<string, object>();
        }

        public RedisDeleteModel(Dictionary<string, object> identifiersAndValuesToDeleteOn)
        {
            this.identifiersAndValuesToDeleteOn = identifiersAndValuesToDeleteOn;
        }

        public string GetDeleteString(IModel model)
        {
            if (identifiersAndValuesToDeleteOn.Count() > 1)
            {
                throw new Exception("Can only delete a value on a single key in Redis.");
            }
            else
            {
                return $"DEL \"{identifiersAndValuesToDeleteOn.First().Key}\"";
            }
        }
    }
}
