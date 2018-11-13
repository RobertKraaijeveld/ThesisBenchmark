using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class CassandraSearchModel<M> : ISearchModel<M> where M : IModel, new()
    {
        public Dictionary<string, object> IdentifiersAndValuesToSearchFor { get; set; }

        public CassandraSearchModel() { }

        public CassandraSearchModel(Dictionary<string, object> IdentifiersAndValuesToSearchFor)
        {
            this.IdentifiersAndValuesToSearchFor = IdentifiersAndValuesToSearchFor;
        }


        public string GetSearchString<T>()
        {
            // SQL's select statement is exactly the same as CQL's, so we just use that :) 
            var sqlSearchModel = new SqlSearchModel<M>(this.IdentifiersAndValuesToSearchFor);
            return sqlSearchModel.GetSearchString<M>();
        }
    }
}
