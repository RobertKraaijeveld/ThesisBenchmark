using System;
using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class CassandraUpdateModel : IUpdateModel
    {
        private readonly Dictionary<string, object> identifiersAndValuesToFilterOn;

        public CassandraUpdateModel(Dictionary<string, object> identifiersAndValuesToFilterOn)
        {
            this.identifiersAndValuesToFilterOn = identifiersAndValuesToFilterOn;
        }


        public string GetUpdateString(IModel newModel)
        {
            // SQL's update statement is exactly the same as CQL's, so we just use that :) 
            var sqlUpdateModel = new SqlUpdateModel(this.identifiersAndValuesToFilterOn);
            return sqlUpdateModel.GetUpdateString(newModel);
        }

        public Dictionary<string, object> GetIdentifiersAndValuesToFilterOn()
        {
            return this.identifiersAndValuesToFilterOn;
        }
    }
}
