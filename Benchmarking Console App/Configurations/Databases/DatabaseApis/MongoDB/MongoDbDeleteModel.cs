using System.Collections.Generic;
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
        public Dictionary<string, object> identifiersAndValuesToDeleteOn { get; set; }

        public MongoDbDeleteModel(Dictionary<string, object> identifiersAndValuesToDeleteOn)
        {
            this.identifiersAndValuesToDeleteOn = identifiersAndValuesToDeleteOn;
        }

        public string GetDeleteString(IModel model)
        {
            var deleteCmdText = $"{{ ";

            // If current identifier is the same as the model's primary key,
            // we exclusively filter on that property (renamed to '_id' in mongodb), for performance reasons.
            var modelPrimaryKeyPropertyName = model.GetPrimaryKeyFieldName();
            if (identifiersAndValuesToDeleteOn.ContainsKey(modelPrimaryKeyPropertyName))
            {
                var primaryKeyValue = identifiersAndValuesToDeleteOn[modelPrimaryKeyPropertyName];

                deleteCmdText += $"_id: {base.ValueToString(primaryKeyValue)}";

                return deleteCmdText += "}"; 
            }

            foreach (var identifierAndValue in identifiersAndValuesToDeleteOn)
            {
                var identifier = identifierAndValue.Key;
                var valueToDeleteOn = identifierAndValue.Value;

                deleteCmdText += $"{identifier}: {base.ValueToString(valueToDeleteOn)},";
            }

            deleteCmdText = deleteCmdText.Remove(deleteCmdText.Length - 1, 1);
            deleteCmdText += "}";

            return deleteCmdText;
        }
    }
}
