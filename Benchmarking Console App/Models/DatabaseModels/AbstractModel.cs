using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Benchmarking_program.Models.DatabaseModels
{
    public abstract class AbstractModel<T> : IModel where T : IModel, new() 
    {
        public abstract T Clone();

        public abstract object GetPrimaryKeyPropertyValue();

        public abstract string GetPrimaryKeyPropertyName();

        public abstract string GetCollectionName();

        public abstract void Randomize(int amountOfExistingModels, Random randomGenerator);

            
        // Is used to map the variable names (column names in the DB) to the appropriate variable values within the class.
        // IE. if a model has a property called Id that contains the value for the "Identifier" column, you would have { "Identifier", this.Id } 
        public Dictionary<string, object> GetFieldsWithValues()
        {
            var type = this.GetType();
            var fieldsOfThisModel = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            var ret = fieldsOfThisModel.ToDictionary(key => key.Name, value => value.GetValue(this));
            return ret;
        }

        // Checks that this model object variable names and types match the names and types of the given table.
        public bool SchemaMatches(Dictionary<string, Type> schemaOfRow)
        {
            // Making sure all schema keys are set to lower
            var type = GetType();
            var fieldsOfThisModel = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach(var field in fieldsOfThisModel)
            {
                var fieldLowerCaseName = field.Name.ToLower();

                bool hasColumnWithSameName = schemaOfRow.ContainsKey(fieldLowerCaseName);
                bool hasSameTypeAsColumn = field.FieldType.Equals(schemaOfRow[fieldLowerCaseName]);
                bool hasANonNullValue = field.GetValue(this) != null;

                if(hasColumnWithSameName && hasSameTypeAsColumn && hasANonNullValue)
                {
                    continue;
                }            
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}