using System;

namespace Benchmarking_program.Models.DatabaseModels
{
    public class SensorValueModel : AbstractModel<SensorValueModel>
    {
        public long SensorValueId;
        public string SensorName;
        public double Value;

        public override SensorValueModel Clone()
        {
            return new SensorValueModel()
            {
                SensorValueId = this.SensorValueId,
                SensorName = this.SensorName,
                Value = this.Value
            };
        }

        public override string GetPrimaryKeyPropertyName()
        {
            return "SensorValueId";
        }

        public override object GetPrimaryKeyPropertyValue()
        {
            return SensorValueId;
        }

        public override string GetCollectionName()
        {
            return "SensorValue";
        }

        public override void Randomize(int amountOfExistingModels, Random randomGenerator)
        {
            SensorValueId = amountOfExistingModels + 1;
            SensorName = "SensorNameHere";
            Value = randomGenerator.NextDouble();
        }
    }
}