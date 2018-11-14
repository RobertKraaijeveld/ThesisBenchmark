using System.Collections.Generic;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class SqlUpdateModel : AbstractSqlOperationModel, IUpdateModel
    {
        public string[] IdentifiersToFilterOn { get; set; }

        public SqlUpdateModel() { }

        public SqlUpdateModel(string[] IdentifiersAndValuesToFilterOn)
        {
            this.IdentifiersToFilterOn = IdentifiersAndValuesToFilterOn;
        }


        public string GetUpdateString(IModel newModel)
        {
            var updateText = $"UPDATE {newModel.GetType().Name.ToLower()} SET ";
            var whereClause = " WHERE ";

            // Creating update clause 
            var modelPrimaryKeyIdentifierName = newModel.GetPrimaryKeyFieldName();
            foreach (var identifierAndValueToUpdateTo in newModel.GetFieldsWithValues())
            {
                var identifierToUpdate = identifierAndValueToUpdateTo.Key;
                var valueToUpdateTo = identifierAndValueToUpdateTo.Value;

                // Not allowing PK's to be updated since some SQL-based DB's (*cough* cassandra *cough*) crash when doing this.
                if (identifierToUpdate.Equals(modelPrimaryKeyIdentifierName) == false)
                {
                    updateText += $"{identifierToUpdate} = {base.ValueToString(valueToUpdateTo)},";
                }
            }

            // Creating where clause
            var newModelsIdentifiersAndValues = newModel.GetFieldsWithValues();
            foreach (var identifierToFilterOn in IdentifiersToFilterOn)
            {
                var identifier = identifierToFilterOn;
                var valueToFilterOn = newModelsIdentifiersAndValues[identifierToFilterOn];

                whereClause += $"{identifier} = {base.ValueToString(valueToFilterOn)} AND";
            }

            // Removing ',' and 'AND' from last item in 'SET' and 'WHERE' portion of the query text.
            // So that 'SET x = y, a = b,' becomes 'SET x = y, a = b' etc. 
            updateText = updateText.Remove(updateText.Length - 1);
            whereClause = whereClause.Remove(whereClause.Length - 4, 4); 

            // Combining update text and where clause, then executing query 
            return updateText += whereClause += ";"; 
        }
    }
}
