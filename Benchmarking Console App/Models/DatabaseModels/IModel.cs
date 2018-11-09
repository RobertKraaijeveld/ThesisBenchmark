using System;
using System.Collections.Generic;

namespace Benchmarking_program.Models.DatabaseModels
{
    public interface IModel
    {
        string GetPrimaryKeyFieldName();

        Dictionary<string, object> GetFieldsWithValues();

        void Randomize(int amountOfExistingModels, Random randomGenerator);

    }
}