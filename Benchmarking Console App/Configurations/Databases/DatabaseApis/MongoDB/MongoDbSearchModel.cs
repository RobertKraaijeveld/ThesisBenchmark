using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.DatabaseApis.MongoDB;
using Benchmarking_program.Configurations.Databases.Interfaces;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class MongoDbSearchModel : AbstractMongoDbOperationModel, ISearchModel
    {
        public List<string> identifiersToRetrieve { get; set; }
        public Dictionary<string, object> identifiersAndValuesToSearchFor { get; set; }

        public MongoDbSearchModel(Dictionary<string, object> identifiersAndValuesToSearchFor, 
                                  List<string> identifiersToRetrieve)
        {
            this.identifiersAndValuesToSearchFor = identifiersAndValuesToSearchFor;
            this.identifiersToRetrieve = identifiersToRetrieve;
        }

        /// <summary>
        /// Returns the full BSON query text for filtering collection @tableName for
        /// values @identifiersAndValuesToSearchFor and retrieving the properties in @identifiersToRetrieve.
        /// </summary>
        public string GetSearchString(string tableName) 
        {
            return $"{{{base.GetQueryText(identifiersAndValuesToSearchFor)}," +
                   $"{base.GetProjectionText(identifiersToRetrieve)}}}";
        }

        
    }
}
