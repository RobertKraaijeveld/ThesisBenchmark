using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;

namespace Benchmarking_Console_App.Tests
{
    public class TestReport
    {
        public string DatabaseTypeUsedStr;
        public string ModelTypeName;

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


        public static void CombineTestReportsIntoCsvFile(List<TestReport> testReports, string fileNameWithoutExtension)
        {
            var currentUnixTime = (Int32) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var fullFileName = $"{currentUnixTime}_{fileNameWithoutExtension}.csv";

            using (var fStream = File.CreateText(GetPathToCsvOutputs() + fullFileName))
            {
                for (int i = 0; i < testReports.Count; i++)
                {
                    var fieldNamesAndValues = testReports[i].GetType()
                                        .GetFields()
                                        .Select(x => new KeyValuePair<string, object>(x.Name, x.GetValue(testReports[i])))
                                        .ToList();

                    // Creating CSV header values 
                    if (i == 0)
                    {
                        var headerLine = string.Join(",", fieldNamesAndValues.Select(x => x.Key));
                        fStream.WriteLine(headerLine);
                    }

                    // Writing values of fields to CSV as new line
                    var valuesLine = string.Join(",", fieldNamesAndValues.Select(x => x.Value));
                    fStream.WriteLine(valuesLine);
                }
            }
        }

        public static string GetPathToCsvOutputs()
        {
            var pathToProject = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            return pathToProject + "\\Output\\";
        }
    }
}
