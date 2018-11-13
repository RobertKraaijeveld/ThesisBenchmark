using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class RedisDeleteModel : IDeleteModel
    {
        public string[] IdentifiersToDeleteOn { get; set; }

        public string GetDeleteString(IModel model)
        {
            if (IdentifiersToDeleteOn.Count() > 1)
            {
                throw new Exception("Can only delete a value on a single key in Redis.");
            }
            else
            {
                return $"DEL \"{IdentifiersToDeleteOn.First()}\"";
            }
        }
    }
}
