using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis.Redis
{
    public class RedisSearchModel<M> : ISearchModel<M> where M : IModel, new()
    {
        public Dictionary<string, object> IdentifiersAndValuesToSearchFor { get; set; }

        public RedisSearchModel() { }

        public RedisSearchModel(Dictionary<string, object> identifiersAndValuesToSearchFor)
        {
            this.IdentifiersAndValuesToSearchFor = identifiersAndValuesToSearchFor;
        }

        public string GetSearchString<M>()
        {
            if (this.IdentifiersAndValuesToSearchFor.Count > 1)
            {
                throw new Exception("Redis is a key-value store and can only search on one identifier->value at the time!");
            }
            else
            {
                var KeyToRetrieve = IdentifiersAndValuesToSearchFor.Single().Value;
                return $"'{KeyToRetrieve}'";
            }
        }
    }
}
