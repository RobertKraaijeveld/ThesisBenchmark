using System;
using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.DatabaseApis.MongoDB;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    /// <summary>
    /// Provides functionality for updating a given IModel in a MongoDB database.
    /// The @IdentifiersAndValuesToFilterOn property is a KV of:
    /// { Key: Column/property name, Value: Value to filter on for this column/property }
    /// </summary>
    public class MongoDbUpdateModel : AbstractMongoDbOperationModel, IUpdateModel
    {
        private readonly Dictionary<string, object> identifiersAndValuesToFilterOn;

        public MongoDbUpdateModel(Dictionary<string, object> identifiersAndValuesToFilterOn)
        {
            this.identifiersAndValuesToFilterOn = identifiersAndValuesToFilterOn;
        }


        public string GetUpdateString(IModel newModel)
        {
            // In MongoDB, updating == creating since the whole document is replaced anyhow.
            var mongoDbCreateModel = new MongoDbCreateModel();

            var filterPortion = base.GetQueryText(this.identifiersAndValuesToFilterOn);
            var updatePortion = mongoDbCreateModel.GetCreateString(newModel);

            // Making sure filter for primary key has name _id instead of name of primary key
            filterPortion = filterPortion.Replace(newModel.GetPrimaryKeyFieldName(), "_id");

            return "{" + filterPortion + "," + updatePortion + "}";
        }

        public Dictionary<string, object> GetIdentifiersAndValuesToFilterOn()
        {
            return this.identifiersAndValuesToFilterOn;
        }
    }
}
