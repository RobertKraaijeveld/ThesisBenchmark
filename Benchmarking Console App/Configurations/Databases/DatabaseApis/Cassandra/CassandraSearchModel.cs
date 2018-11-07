using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class CassandraSearchModel : ISearchModel
    {
        public List<string> identifiersToRetrieve { get; set; }
        public Dictionary<string, object> identifiersAndValuesToSearchFor { get; set; }

        public CassandraSearchModel()
        {
            this.identifiersToRetrieve = new List<string>();
            this.identifiersAndValuesToSearchFor = new Dictionary<string, object>();
        }

        public CassandraSearchModel(Dictionary<string, object> identifiersAndValuesToSearchFor, 
                                    List<string> identifiersToRetrieve)
        {
            this.identifiersAndValuesToSearchFor = identifiersAndValuesToSearchFor;
            this.identifiersToRetrieve = identifiersToRetrieve;
        }


        public string GetSearchString(string tableName) 
        {
            // SQL's select statement is exactly the same as CQL's, so we just use that :) 
            var sqlSearchModel = new SqlSearchModel(this.identifiersToRetrieve, this.identifiersAndValuesToSearchFor);
            return sqlSearchModel.GetSearchString(tableName);
        }
    }
}
