using System;
using System.Linq;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class SqlDeleteModel : AbstractSqlOperationModel, IDeleteModel
    {
        public string[] IdentifiersToDeleteOn { get; set; }

        public SqlDeleteModel() { }

        public SqlDeleteModel(string[] identifiersToDeleteOn)
        {
            this.IdentifiersToDeleteOn = identifiersToDeleteOn;
        }

        public string GetDeleteString(IModel model)
        {
            var identifierAndValuesOfModel = model.GetFieldsWithValues();

            var deleteText = $"DELETE FROM {model.GetType().Name.ToLower()}";

            if (this.IdentifiersToDeleteOn.Any())
            {
                var whereClause = " WHERE ";
                foreach (var identifier in IdentifiersToDeleteOn)
                {
                    if (identifierAndValuesOfModel.ContainsKey(identifier))
                    {
                        var modelsValueForIdentifier = identifierAndValuesOfModel[identifier];
                        whereClause += $"{identifier} = {base.ValueToString(modelsValueForIdentifier)} AND";
                    }
                    else throw new Exception("Cannot delete using a property that model does not have.");
                }

                whereClause = whereClause.Remove(whereClause.Length - 4, 4);
                deleteText += whereClause += ";";
            }

            return deleteText;
        }
    }
}
