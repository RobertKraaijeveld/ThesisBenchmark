using System;
using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class SqlDeleteModel : IDeleteModel
    {
        public Dictionary<string, object> identifiersAndValuesToDeleteOn { get; set; }

        public SqlDeleteModel(Dictionary<string, object> identifiersAndValuesToDeleteOn)
        {
            this.identifiersAndValuesToDeleteOn = identifiersAndValuesToDeleteOn;
        }

        public string GetDeleteString(IModel model)
        {
            var propertiesAndValuesOfModel = model.GetFieldsWithValues();

            var deleteText = $"DELETE FROM {model.GetType().Name.ToLower()}";
            var whereClause = " WHERE ";

            foreach (var identifierAndValueKv in identifiersAndValuesToDeleteOn)
            {
                if (propertiesAndValuesOfModel.ContainsKey(identifierAndValueKv.Key))
                {
                    whereClause += $"{identifierAndValueKv.Key} = {identifierAndValueKv.Value} AND";
                }
                else throw new Exception("Cannot delete using a property that model does not have.");
            }

            whereClause = whereClause.Remove(whereClause.Length - 4, 4); 
            return deleteText += whereClause += ";"; 
        }
    }
}
