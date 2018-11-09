using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Benchmarking_Console_App.Configurations.Databases.Attributes;

namespace Benchmarking_program.Models.DatabaseModels
{
    public abstract class AbstractModel<T> : IModel where T : IModel, new() 
    {
        public abstract void Randomize(int amountOfExistingModels, Random randomGenerator);


        public string GetPrimaryKeyFieldName()
        {
            var publicFields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public);

            if ()
            {

            }


            throw new Exception("Model must have primary key, using [IsPrimaryKey] attribute!");

            // The primary key field is the one which has the 'IsPrimaryKey' attribute.
            return 
                            .Where(p => p.CustomAttributes.Any(x => x.AttributeType == typeof(IsPrimaryKey)))
                            .Select(x => x.Name)
                            .First();
        }

        // Is used to map the variable names (column names in the DB) to the appropriate variable values within the class.
        // IE. if a model has a property called Id that contains the value for the "Identifier" column, you would have { "Identifier", this.Id } 
        public Dictionary<string, object> GetFieldsWithValues()
        {
            var type = this.GetType();
            var fieldsOfThisModel = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            var ret = fieldsOfThisModel.ToDictionary(key => key.Name, value => value.GetValue(this));
            return ret;
        }
    }
}