using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class CassandraCreateModel : ICreateModel
    {
        public string GetCreateString(IModel model)
        {
            // SQL's create statement is exactly the same as CQL's, so we just use that :) 
            var sqlCreateModel =  new SqlCreateModel();
            return sqlCreateModel.GetCreateString(model);
        }
    }
}
