using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class CassandraSearchModel<M> : ISearchModel<M> where M : IModel, new()
    {
        public List<string> identifiersToRetrieve { get; set; }
        public Dictionary<string, object> identifiersAndValuesToSearchFor { get; set; }

        public CassandraSearchModel(Dictionary<string, object> identifiersAndValuesToSearchFor, 
                                    List<string> identifiersToRetrieve)
        {
            this.identifiersAndValuesToSearchFor = identifiersAndValuesToSearchFor;
            this.identifiersToRetrieve = identifiersToRetrieve;
        }


        public string GetSearchString<T>()
        {
            // SQL's select statement is exactly the same as CQL's, so we just use that :) 
            var sqlSearchModel = new SqlSearchModel<M>(this.identifiersToRetrieve, this.identifiersAndValuesToSearchFor);
            return sqlSearchModel.GetSearchString<M>();
        }
    }
}
