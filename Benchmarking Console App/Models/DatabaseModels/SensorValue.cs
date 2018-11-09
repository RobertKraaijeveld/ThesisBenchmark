using System;
using Benchmarking_Console_App.Configurations.Databases.Attributes;

namespace Benchmarking_program.Models.DatabaseModels
{
    public class SensorValue : AbstractModel<SensorValue>
    {
        [IsPrimaryKey]
        public long SensorValueId;

        public string SensorName;

        public double Value;

        public override void Randomize(int amountOfExistingModels, Random randomGenerator)
        {
            SensorValueId = amountOfExistingModels + 1;
            SensorName = "SensorNameHere";
            Value = randomGenerator.NextDouble();
        }
    }
}