using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Benchmarking_Console_App.Configurations.Databases.Attributes;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis.Cassandra
{
    public class CassandraCreateCollectionModel<M> : ICreateCollectionModel<M> where M: IModel, new()
    {
        private readonly string keySpaceName;

        private readonly Dictionary<Type, string> _typesToSqlType = new Dictionary<Type, string>()
        {
            { typeof(int), "int" },
            { typeof(long), "bigint" },
            { typeof(string), "text" },
            { typeof(double), "decimal" },
            { typeof(DateTime), "date" }
        };

        public CassandraCreateCollectionModel(string keyspaceName)
        {
            this.keySpaceName = keyspaceName;
        }


        public string GetCreateCollectionText()
        {
            var modelFieldsNamesAndTypes = this.GetNamesAndTypesOfModelFields<M>();
            var primaryKeyFieldsName = this.GetPrimaryKeyNameFromType<M>();

            string cqlStr = $"CREATE TABLE {this.keySpaceName}.{typeof(M).Name.ToLower()} (";
            foreach (var fieldNameAndType in modelFieldsNamesAndTypes)
            {
                var columnName = fieldNameAndType.Key;
                var columnType = fieldNameAndType.Value;

                var sqlStringForColumnType = this._typesToSqlType[columnType];

                // Adding column name and type and primary key constraint
                cqlStr += $"{columnName.ToLower()} ";
                cqlStr += $" {sqlStringForColumnType} ";

                if (primaryKeyFieldsName.Equals(columnName))
                {
                    cqlStr += "PRIMARY KEY";    
                }
                cqlStr += ",";
            }

            // Removing last comma
            cqlStr = cqlStr.Remove(cqlStr.Length - 1, 1);
            
            return cqlStr += ");";
        }

        private Dictionary<string, Type> GetNamesAndTypesOfModelFields<M>() where M : IModel, new()
        {
            return typeof(M).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .ToDictionary(k => k.Name, v => v.PropertyType);
        }

        private string GetPrimaryKeyNameFromType<M>() where M : IModel, new()
        {
            return typeof(M).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .Where(p => p.CustomAttributes.Any(x => x.AttributeType == typeof(IsPrimaryKey)))
                            .Select(x => x.Name)
                            .Single();
        }
    }
}
