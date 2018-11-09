using System.Collections.Generic;
using System.Linq;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class SqlSearchModel<M> : ISearchModel<M> where M: IModel, new()
    {
        public List<string> identifiersToRetrieve { get; set; }
        public Dictionary<string, object> identifiersAndValuesToSearchFor { get; set; }

        public SqlSearchModel(List<string> identifiersToRetrieve,
                              Dictionary<string, object> identifiersAndValuesToSearchFor)
        {
            this.identifiersToRetrieve = identifiersToRetrieve;
            this.identifiersAndValuesToSearchFor = identifiersAndValuesToSearchFor;
        }


        public string GetSearchString<M>()
        {
            var selectClause = $"SELECT "; 
            var fromClause = $" FROM {nameof(M).ToLower()} ";
            var whereClause = "";

            foreach (var identifier in identifiersToRetrieve)
            {
                selectClause += $"{identifier},";
            }

            if (identifiersAndValuesToSearchFor.Any())
            {
                whereClause = " WHERE ";
                foreach (var identifierAndValueKv in identifiersAndValuesToSearchFor)
                {
                    whereClause += $"{identifierAndValueKv.Key} = {identifierAndValueKv.Value} AND";
                }

                // Removing last 'AND' from where clause
                whereClause = whereClause.Remove(whereClause.Length - 4, 4);

            }

            // Removing last comma from select clause
            selectClause = selectClause.Remove(selectClause.Length - 1);

            return selectClause += fromClause += whereClause += ";";
        }
    }
}
