using Benchmarking_program.Configurations.Databases.DatabaseApis.MongoDB;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;
using Newtonsoft.Json;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class MongoDbCreateModel : AbstractMongoDbOperationModel, ICreateModel
    {
        public string GetCreateString(IModel model)
        {
            var createCmdText = "{";
            var primaryKeyOfModel = model.GetPrimaryKeyFieldName();
            
            foreach (var propertyAndValue in model.GetFieldsWithValues())
            {
                var propertyName = propertyAndValue.Key;
                var propertyValue = propertyAndValue.Value;

                createCmdText += $"{propertyName}: {base.ValueToString(propertyValue)},";
            }

            createCmdText = createCmdText.Remove(createCmdText.Length - 1, 1); // removing last comma
            return createCmdText += "}";
        }
    }
}
