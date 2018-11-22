using System.Collections.Generic;
using System.Linq;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class SqlSearchModel<M> : AbstractSqlOperationModel, ISearchModel<M> where M: IModel, new()
    {
        public Dictionary<string, object> IdentifiersAndValuesToSearchFor { get; set; }

        public SqlSearchModel() { }

        public SqlSearchModel(Dictionary<string, object> identifiersAndValuesToSearchFor)
        {
            this.IdentifiersAndValuesToSearchFor = identifiersAndValuesToSearchFor;
        }

        public ISearchModel<M> Clone()
        {
            return new SqlSearchModel<M>(this.IdentifiersAndValuesToSearchFor);
        }


        public string GetSearchString<M>()
        {
            var selectClause = $"SELECT *"; 
            var fromClause = $" FROM {typeof(M).Name.ToLower()} ";
            var whereClause = "";

            if (IdentifiersAndValuesToSearchFor.Any())
            {
                whereClause = " WHERE ";
                foreach (var identifierAndValueKv in IdentifiersAndValuesToSearchFor)
                {
                    whereClause += $"{identifierAndValueKv.Key} = {base.ValueToString(identifierAndValueKv.Value)} AND";
                }

                // Removing last 'AND' from where clause
                whereClause = whereClause.Remove(whereClause.Length - 4, 4);
            }

            return selectClause += fromClause += whereClause += ";";
        }
    }
}
