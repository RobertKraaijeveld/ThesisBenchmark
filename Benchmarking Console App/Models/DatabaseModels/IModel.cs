using System;
using System.Collections.Generic;

namespace Benchmarking_program.Models.DatabaseModels
{
    public interface IModel
    {
        string GetPrimaryKeyPropertyName();
        object GetPrimaryKeyPropertyValue();
        string GetCollectionName();

        void Randomize(int amountOfExistingModels, Random randomGenerator);

        Dictionary<string, object> GetFieldsWithValues();
        bool SchemaMatches(Dictionary<string, Type> schemaOfRow);
    }
}