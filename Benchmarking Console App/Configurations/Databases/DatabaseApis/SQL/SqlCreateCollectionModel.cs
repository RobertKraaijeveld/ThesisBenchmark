using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Benchmarking_Console_App.Configurations.Databases.Attributes;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL
{
    public class SqlCreateCollectionModel<M> : ICreateCollectionModel<M> where M: IModel, new()
    {
        private readonly Dictionary<Type, string> _typesToSqlType = new Dictionary<Type, string>()
        {
            { typeof(int), "INT" },
            { typeof(long), "BIGINT" },
            { typeof(string), "VARCHAR(45)" },
            { typeof(double), "DECIMAL" },
            { typeof(DateTime), "DATE" }
        };

        public string GetCreateCollectionText()
        {
            var modelFieldsNamesAndTypes = this.GetNamesAndTypesOfModelProperties<M>();

            string sqlStr = $"CREATE TABLE {typeof(M).Name.ToLower()} (";
            foreach(var fieldNameAndType in modelFieldsNamesAndTypes)
            {
                var columnName = fieldNameAndType.Key;
                var columnType = fieldNameAndType.Value;

                var sqlStringForColumnType = this._typesToSqlType[columnType];

                sqlStr += $"{columnName.ToLower()} {sqlStringForColumnType} NOT NULL,";

            }

            // Adding primary key constraint if any is specified, otherwise first field is used.
            sqlStr += "PRIMARY KEY(";
            string primaryKeyName;
            if (HasPrimaryKeySpecified<M>())
            {
                primaryKeyName = GetPrimaryKeyNameFromType<M>();
            }
            else
            {
                var firstFieldNameAndType = modelFieldsNamesAndTypes.First();
                primaryKeyName = firstFieldNameAndType.Key;
            }
            sqlStr += $"{primaryKeyName.ToLower()}";

            return sqlStr += "));";
        }

        private Dictionary<string, Type> GetNamesAndTypesOfModelProperties<M>() where M: IModel, new()
        {
            return typeof(M).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .ToDictionary(k => k.Name, v => v.PropertyType);
        }

        private bool HasPrimaryKeySpecified<M>() where M : IModel, new()
        {
            return typeof(M).CustomAttributes.Any(x => x.AttributeType == typeof(IsPrimaryKey));
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
