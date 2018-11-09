using System;
using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class SqlUpdateModel : IUpdateModel
    {
        private readonly Dictionary<string, object> identifiersAndValuesToFilterOn;

        public SqlUpdateModel(Dictionary<string, object> identifiersAndValuesToFilterOn)
        {
            this.identifiersAndValuesToFilterOn = identifiersAndValuesToFilterOn;
        }


        public string GetUpdateString(IModel newModel)
        {
            var updateText = $"UPDATE {newModel.GetType().Name.ToLower()} SET ";
            var whereClause = " WHERE ";

            // Creating update clause TODO: Check for nulls
            foreach (var identifierAndValueToUpdateTo in newModel.GetFieldsWithValues())
            {
                var identifierToUpdate = identifierAndValueToUpdateTo.Key;
                var valueToUpdateTo = identifierAndValueToUpdateTo.Value;

                updateText += $"{identifierToUpdate} = {valueToUpdateTo},";
            }

            // Creating where clause
            foreach (var identifierAndValueToFilterOn in identifiersAndValuesToFilterOn)
            {
                var identifier = identifierAndValueToFilterOn.Key;
                var valueToFilterOn = identifierAndValueToFilterOn.Value;

                whereClause += $"{identifier} = {valueToFilterOn} AND";
            }

            // Removing ',' and 'AND' from last item in 'SET' and 'WHERE' portion of the query text.
            // So that 'SET x = y, a = b,' becomes 'SET x = y, a = b' etc. 
            updateText = updateText.Remove(updateText.Length - 1);
            whereClause = whereClause.Remove(whereClause.Length - 4, 4); 

            // Combining update text and where clause, then executing query 
            return updateText += whereClause += ";"; 
        }

        public Dictionary<string, object> GetIdentifiersAndValuesToFilterOn()
        {
            return this.identifiersAndValuesToFilterOn;
        }
    }
}
