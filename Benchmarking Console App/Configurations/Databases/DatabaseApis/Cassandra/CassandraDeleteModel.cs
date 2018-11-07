using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class CassandraDeleteModel : IDeleteModel
    {
        public Dictionary<string, object> identifiersAndValuesToDeleteOn { get; set; }

        public CassandraDeleteModel()
        {
            this.identifiersAndValuesToDeleteOn = new Dictionary<string, object>();
        }

        public CassandraDeleteModel(Dictionary<string, object> identifiersAndValuesToDeleteOn)
        {
            this.identifiersAndValuesToDeleteOn = identifiersAndValuesToDeleteOn;
        }

        public string GetDeleteString(IModel model)
        {
            // SQL's delete statement is exactly the same as CQL's, so we just use that :) 
            var sqlDeleteModel = new SqlDeleteModel(this.identifiersAndValuesToDeleteOn);
            return sqlDeleteModel.GetDeleteString(model);
        }
    }
}
