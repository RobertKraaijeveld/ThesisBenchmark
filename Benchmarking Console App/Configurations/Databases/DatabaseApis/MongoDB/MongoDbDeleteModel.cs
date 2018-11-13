using System.Collections.Generic;
using System.Linq;
using Benchmarking_program.Configurations.Databases.DatabaseApis.MongoDB;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    /// <summary>
    /// Provides functionality to delete an IModel from a MongoDB database.
    /// </summary>
    public class MongoDbDeleteModel : AbstractMongoDbOperationModel, IDeleteModel
    {
        public string[] IdentifiersToDeleteOn { get; set; }

        public MongoDbDeleteModel() { }

        public MongoDbDeleteModel(string[] IdentifiersToDeleteOn)
        {
            this.IdentifiersToDeleteOn = IdentifiersToDeleteOn;
        }

        public string GetDeleteString(IModel model)
        {
            var deleteCmdText = $"{{ ";

            // If current identifier is the same as the model's primary key,
            // we exclusively filter on that property (renamed to '_id' in mongodb), for performance reasons.
            var modelsIdentifiersAndValues = model.GetFieldsWithValues();
            var modelPrimaryKeyPropertyName = model.GetPrimaryKeyFieldName();

            if (IdentifiersToDeleteOn.ToList().Contains(modelPrimaryKeyPropertyName))
            {
                var primaryKeyValue = modelsIdentifiersAndValues[modelPrimaryKeyPropertyName];

                deleteCmdText += $"_id: {base.ValueToString(primaryKeyValue)}";

                return deleteCmdText += "}"; 
            }

            foreach (var identifier in IdentifiersToDeleteOn)
            {
                var valueToDeleteOn = modelsIdentifiersAndValues[identifier];
                deleteCmdText += $"{identifier}: {base.ValueToString(valueToDeleteOn)},";
            }

            deleteCmdText = deleteCmdText.Remove(deleteCmdText.Length - 1, 1);
            deleteCmdText += "}";

            return deleteCmdText;
        }
    }
}
