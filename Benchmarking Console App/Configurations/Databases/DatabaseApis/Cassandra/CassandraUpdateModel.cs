using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class CassandraUpdateModel : IUpdateModel
    {
        public string[] IdentifiersToFilterOn { get; set; }

        public CassandraUpdateModel() { }

        public CassandraUpdateModel(string[] IdentifiersToFilterOn)
        {
            this.IdentifiersToFilterOn = IdentifiersToFilterOn;
        }


        public string GetUpdateString(IModel newModel)
        {
            // SQL's update statement is exactly the same as CQL's, so we just use that :) 
            var sqlUpdateModel = new SqlUpdateModel(this.IdentifiersToFilterOn);
            return sqlUpdateModel.GetUpdateString(newModel);
        }
    }
}
