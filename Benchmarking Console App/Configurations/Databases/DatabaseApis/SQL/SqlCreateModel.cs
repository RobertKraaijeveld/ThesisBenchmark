using System;
using System.Linq;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class SqlCreateModel : ICreateModel
    {
        public string GetCreateString(IModel model)
        {
            // Creating SQL statement text
            var modelColumnsAndValuesDict = model.GetFieldsWithValues();

            string sqlStr = $"INSERT INTO {model.GetType().Name.ToLower()} "; 
            string columnNamesStr = "(";
            string valuesStr = " VALUES (";

            for (int i = 0; i < modelColumnsAndValuesDict.Count; i++)
            {
                // Constructing columns/values part of SQL
                columnNamesStr += $"{modelColumnsAndValuesDict.Keys.ElementAt(i)}";


                // Making sure that string values are quoted
                object parameterValue = modelColumnsAndValuesDict.Values.ElementAt(i);
                var type = parameterValue.GetType();

                if (type == typeof(String)) valuesStr += $"'{parameterValue}'";
                else valuesStr += parameterValue;


                columnNamesStr += ",";
                valuesStr += ",";
            }

            // Removing last comma, taking care of closing braces etc.
            columnNamesStr = columnNamesStr.Remove(columnNamesStr.Length - 1);
            columnNamesStr += ") ";

            valuesStr = valuesStr.Remove(valuesStr.Length - 1);
            valuesStr += ") ";


            return sqlStr += columnNamesStr += valuesStr += ";";
        }
    }
}
