using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis.Cassandra
{
    public class CassandraGetAllModel<M> : IGetAllModel<M> where M : IModel, new()
    {
        public string CreateGetAllString()
        {
            // Same in SQL as in Cassandra so we just re-use the SQL model
            return new SqlGetAllModel<M>().CreateGetAllString();
        }
    }
}
