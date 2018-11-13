using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;

namespace Benchmarking_Console_App.Tests.TestReport
{
    public class TestReport
    {
        public EDatabaseType DatabaseTypeUsed;
        public Type ModelTypeUsed;

        public int AmountOfModelsInserted;
        public double TimeSpentInsertingModels;

        public double TimeSpentRetrievingAllModels;

        public int AmountOfModelsRetrievedByPrimaryKey;
        public double TimeSpentRetrievingModelsByPrimaryKey;

        public int AmountOfModelsRetrievedByContent;
        public double TimeSpentRetrievingModelsByContent;

        public int AmountOfModelsUpdated;
        public int FieldsUpdatedPerModel;
        public double TimeSpentUpdatingModels;

        public double TimeSpentDeletingAllModels;


        public void ToCsvFile(string fileName)
        {
            using (var fs = File.CreateText(fileName))
            {
                var fieldNamesAndValues = this.GetType()
                                              .GetFields()
                                              .Select(x => new KeyValuePair<string, object>(x.Name, x.GetValue(this)))
                                              .ToArray();

                for (int i = 0; i < fieldNamesAndValues.Count() * 2; i++)
                {
                    if (i <= fieldNamesAndValues.Count())
                    {
                        fs.Write(fieldNamesAndValues[i].Key);
                    }
                    else // Done writing names into first line, now writing values into second line
                    {
                        fs.Write(fieldNamesAndValues[i].Value);
                    }
                }
            }
        }
    }
}
