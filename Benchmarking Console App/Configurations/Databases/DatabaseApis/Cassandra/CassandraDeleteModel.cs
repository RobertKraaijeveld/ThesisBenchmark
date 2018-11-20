using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class CassandraDeleteModel : IDeleteModel
    {
        public string[] IdentifiersToDeleteOn { get; set; }
        

        public string GetDeleteString(IModel model)
        {
            // SQL's delete statement is exactly the same as CQL's, so we just use that :) 
            var sqlDeleteModel = new SqlDeleteModel(this.IdentifiersToDeleteOn);
            return sqlDeleteModel.GetDeleteString(model);
        }
    }
}
