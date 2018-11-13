using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.DatabaseApis.MongoDB;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class MongoDbSearchModel<M> : AbstractMongoDbOperationModel, ISearchModel<M> where M : IModel, new()
    {
        public Dictionary<string, object> IdentifiersAndValuesToSearchFor { get; set; }

        public MongoDbSearchModel() { }

        public MongoDbSearchModel(Dictionary<string, object> identifiersAndValuesToSearchFor, 
                                  List<string> identifiersToRetrieve)
        {
            this.IdentifiersAndValuesToSearchFor = identifiersAndValuesToSearchFor;
        }

        /// <summary>
        /// Returns the full BSON query text for filtering collection @tableName for
        /// values @identifiersAndValuesToSearchFor and retrieving the properties in @identifiersToRetrieve.
        /// </summary>
        public string GetSearchString<M>() 
        {
            return $"{{{base.GetQueryText(IdentifiersAndValuesToSearchFor)}," +
                   $"{{ }} }}"; // Empty projection == SELECT *
        }
    }
}
