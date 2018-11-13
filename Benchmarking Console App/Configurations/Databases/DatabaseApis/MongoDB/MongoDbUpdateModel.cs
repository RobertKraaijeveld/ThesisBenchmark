using System.Collections.Generic;
using System.Linq;
using Benchmarking_program.Configurations.Databases.DatabaseApis.MongoDB;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    /// <summary>
    /// Provides functionality for updating a given IModel in a MongoDB database.
    /// The @IdentifiersToFilterOn property is a KV of:
    /// { Key: Column/property name, Value: Value to filter on for this column/property }
    /// </summary>
    public class MongoDbUpdateModel : AbstractMongoDbOperationModel, IUpdateModel
    {
        public string[] IdentifiersToFilterOn { get; set; }

        public MongoDbUpdateModel() { }

        public MongoDbUpdateModel(string[] IdentifiersToFilterOn)
        {
            this.IdentifiersToFilterOn = IdentifiersToFilterOn;
        }


        public string GetUpdateString(IModel newModel)
        {
            // Creating the 'filter' portion of the query by making a dict out of the identifiers that we were
            // told to filter on, plus the values that the model has for those identifiers.
            var newModelValuesAndIdentifiers = newModel.GetFieldsWithValues();
            var identifiersAndValuesToFilterOn = this.IdentifiersToFilterOn
                                                     .ToDictionary(key => key, value => newModelValuesAndIdentifiers[value]);
            var filterPortion = base.GetQueryText(identifiersAndValuesToFilterOn);

            // In MongoDB, updating == creating since the whole document is replaced anyhow.
            var mongoDbCreateModel = new MongoDbCreateModel();
            var updatePortion = mongoDbCreateModel.GetCreateString(newModel);

            // Making sure filter for primary key has name _id instead of name of primary key
            filterPortion = filterPortion.Replace(newModel.GetPrimaryKeyFieldName(), "_id");

            return "{" + filterPortion + "," + updatePortion + "}";
        }
    }
}
