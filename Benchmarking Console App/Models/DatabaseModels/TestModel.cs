using System;

namespace Benchmarking_program.Models.DatabaseModels
{
    /// <summary>
    /// <B>TEMPORARY</B>, model for Test table.
    /// </summary>
    public class TestModel : AbstractModel<TestModel>
    {
        public long Id;
        public double Value;

        public override TestModel Clone()
        {
            return new TestModel()
            {
                Id = this.Id
            };
        }

        public override string GetPrimaryKeyPropertyName()
        {
            return "Id";
        }

        public override object GetPrimaryKeyPropertyValue()
        {
            return Id;
        }

        public override string GetCollectionName()
        {
            return "test";
        }

        public override void Randomize(int amountOfExistingModels, Random randomGenerator)
        {
            Id = amountOfExistingModels + 1;
            Value = randomGenerator.NextDouble();
        }
    }
}
