using System;
using System.Collections.Generic;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.MongoDB
{
    /// <summary>
    /// Contains some utility methods that are used exclusively by the CRUD Models for MongoDB.
    /// </summary>
    public abstract class AbstractMongoDbOperationModel
    {
        protected string ValueToString(object value)
        {
            var type = value.GetType();

            if (type == typeof(String)) return $"\"{value}\"";
            else return value.ToString();
        }

        protected string GetQueryText(Dictionary<string, object> identifiersAndValuesToSearchFor)
        {
            var queryText = "{";

            int counter = 0;
            foreach (var identifierAndValue in identifiersAndValuesToSearchFor)
            {
                var identifier = identifierAndValue.Key;
                var valueToDeleteOn = identifierAndValue.Value;

                queryText += $"{identifier}: {this.ValueToString(valueToDeleteOn)},";
            }

            if(queryText.Length > 1) queryText = queryText.Remove(queryText.Length - 1, 1);

            queryText += "}";
            return queryText;
        }
    }
}
