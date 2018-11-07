using System.Collections.Generic;
using System.Linq;
using Benchmarking_program.Configurations.Databases.Interfaces;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class SqlSearchModel : ISearchModel
    {
        public List<string> identifiersToRetrieve { get; set; }
        public Dictionary<string, object> identifiersAndValuesToSearchFor { get; set; }

        public SqlSearchModel()
        {
            this.identifiersToRetrieve = new List<string>();
            this.identifiersAndValuesToSearchFor = new Dictionary<string, object>();
        }

        public SqlSearchModel(List<string> identifiersToRetrieve,
                              Dictionary<string, object> identifiersAndValuesToSearchFor)
        {
            this.identifiersToRetrieve = identifiersToRetrieve;
            this.identifiersAndValuesToSearchFor = identifiersAndValuesToSearchFor;
        }


        public string GetSearchString(string tableName)
        {
            var selectClause = $"SELECT "; 
            var fromClause = $" FROM {tableName} ";

            foreach (var identifier in identifiersToRetrieve)
            {
                selectClause += $"{identifier},";
            }

            // trimming last comma or 'AND'
            selectClause = selectClause.Remove(selectClause.Length - 1);

            // Adding where clause if identifiersAndValuesToSearchFor contains any values. Otherwise
            if (identifiersAndValuesToSearchFor.Any())
            {
                var whereClause = " WHERE ";

                // where clause
                foreach (var identifierAndValueKv in identifiersAndValuesToSearchFor)
                {
                    whereClause += $"{identifierAndValueKv.Key} = {identifierAndValueKv.Value} AND";
                }

                whereClause = whereClause.Remove(whereClause.Length - 4, 4);

            }



            return selectClause += fromClause += whereClause += ";";
        }
    }
}
