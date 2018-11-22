using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;


namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    /// <summary>
    /// Provides basic CRUD functionality for a MongoDB database.
    /// </summary>
    public class SimpleMongoDbDatabaseApi : IDatabaseApi
    {
        private readonly string _connectionString;
        private readonly MongoClient _mongodbConnection;
        private readonly IMongoDatabase _database;

        public SimpleMongoDbDatabaseApi(string connectionString)
        {
            _connectionString = connectionString;

            _mongodbConnection = new MongoClient(_connectionString);
            _database = _mongodbConnection.GetDatabase("BenchmarkDB");
        }


        public void OpenConnection()
        {
        }

        public void CloseConnection()
        {
        }


        public List<M> Search<M>(List<ISearchModel<M>> searchModels) where M : IModel, new()
        {
            var searchQueries = new List<string>();
            searchModels.ForEach(s => searchQueries.Add(s.GetSearchString<M>()));

            var filtersAndProjection = MultipleBsonToSingleFilterAndProjection(searchQueries);
            return this.GetResults<M>(filtersAndProjection);
        }

        public bool Exists<M>(M model) where M : IModel, new()
        {
            var identifiersAndValuesToSearchFor = model.GetFieldsWithValues();
            var searchModel = new MongoDbSearchModel<M>(identifiersAndValuesToSearchFor);

            return this.Search<M>(new List<ISearchModel<M>> {searchModel}).Any();
        }

        public int Amount<M>() where M : IModel, new()
        {
            return (int) _database.GetCollection<M>(nameof(M))
                .CountDocuments(FilterDefinition<M>.Empty);
        }


        public void Create<M>(List<M> newModels, ICreateModel createModel) where M : IModel, new()
        {
            List<M> deserializedModels = new List<M>();

            foreach (var model in newModels)
            {
                var createQueryText = createModel.GetCreateString(model);
                var deserializedVal = BsonSerializer.Deserialize<M>(createQueryText);

                deserializedModels.Add(deserializedVal);
            }
            _database.GetCollection<M>(nameof(M)).InsertMany(deserializedModels);
        }

        public void Update<M>(List<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new()
        {
            var createModel = new MongoDbCreateModel();
            var deleteModel = new MongoDbDeleteModel(updateModel.IdentifiersToFilterOn);

            foreach (var model in modelsWithNewValues)
            {
                var modelAsList = new List<M> {model};

                this.Delete(modelAsList, deleteModel);
                this.Create(modelAsList, createModel);
            }
        }

        public void Delete<M>(List<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new()
        {
            foreach (var model in modelsToDelete)
            {
                var deleteQueryText = deleteModel.GetDeleteString(model);
                var asBsonDoc = BsonSerializer.Deserialize<BsonDocument>(deleteQueryText);

                _database.GetCollection<M>(nameof(M))
                    .DeleteOne(asBsonDoc);
            }
        }

        public void TruncateAll()
        {
            _database.ListCollectionNames()
                     .ForEachAsync(c => _database.DropCollection(c)); 
        }

        public void Truncate<M>() where M: IModel, new()
        {
            _database.DropCollection(typeof(M).Name);
        }


        //FilterAndProjection separateFilterAndProjection
        private List<M> GetResults<M>(FilterAndProjection filterAndProjection) where M : IModel, new()
        {
            var results = new List<M>();


            var cursor = _database.GetCollection<M>(nameof(M))
                                  .Find(filterAndProjection.Filter)
                                  .Project(filterAndProjection.Projection)
                                  .ToCursor();

            while (cursor.MoveNext())
            {
                var currentBatchOfDocuments = cursor.Current;
                foreach (var document in currentBatchOfDocuments)
                {
                    document.Remove("_id");
                    results.Add(BsonSerializer.Deserialize<M>(document));
                }
            }
            return results;
        }

        // Takes a collection of searchModel output strings, each in the form of { "attributeName": "valueToFilterOn" } 
        // And turns them into { "attributeName1": { $in: ["value1", "value2"] }, .. etc } 
        private FilterAndProjection MultipleBsonToSingleFilterAndProjection(List<string> bsonStrings)
        {
            // First, turning the list of strings into a list of filter:projection pairs. 
            List<FilterAndProjection> allFilterAndProjections = new List<FilterAndProjection>();
            bsonStrings.ForEach(x => allFilterAndProjections.Add(GetSeparateFilterAndProjection(x)));

            // Then, getting the values that we want to filter on PER attribute, as a dict<string, list<string>>
            var attributesAndValues = new Dictionary<string, List<string>>();

            foreach (var filterAndProjection in allFilterAndProjections)
            {
                var key = filterAndProjection.Filter.Elements.First().Name;
                var val = filterAndProjection.Filter.Elements.First().Value.ToString();

                if (attributesAndValues.ContainsKey(key))
                {
                    attributesAndValues[key].Add(val);
                }
                else
                {
                    attributesAndValues.Add(key, new List<string> { val });
                }
            }

            // Finally, constructing the complete filtering string out of that.\
            string projection = "{}"; // todo fixme
            string flattenedFilterString = "";
            foreach (var attributeAndValues in attributesAndValues)
            {
                var attribute = attributeAndValues.Key;
                flattenedFilterString += $"{{ {attribute}: {{ $in: [";

                foreach (var val in attributeAndValues.Value)
                {
                    flattenedFilterString += $"{val},";
                }

                // removing trailing comma
                flattenedFilterString = flattenedFilterString.Remove(flattenedFilterString.Length - 1);

                flattenedFilterString += $"] }} }}";
            }

            // Combining projection and filter
            return new FilterAndProjection()
            {
                Filter = BsonSerializer.Deserialize<BsonDocument>(flattenedFilterString),
                Projection = BsonSerializer.Deserialize<BsonDocument>(projection)
            };
        }

        private FilterAndProjection GetSeparateFilterAndProjection(string queryWithFilterAndProjectionJson)
        {
            // string looks like { {<filter>}, {<update>} }
            // removing first '{' and last '}' to make the rest of parsing easier
            var firstBraceIx = queryWithFilterAndProjectionJson.IndexOf('{');
            var lastBraceIx = queryWithFilterAndProjectionJson.LastIndexOf('}');

            queryWithFilterAndProjectionJson = queryWithFilterAndProjectionJson.Remove(firstBraceIx, 1);
            queryWithFilterAndProjectionJson = queryWithFilterAndProjectionJson.Remove(lastBraceIx - 1, 1);

            // splitting the string to get the portions within the two '{ .. }' sections.
            // note that this means this code does not work with sub-filters/updates, ie. '{ {<filter>} {<update>{<more updates>}<update>} }
            var separatedFilterAndProjection = queryWithFilterAndProjectionJson.Split('{', '}');

            return new FilterAndProjection()
            {
                Filter = BsonDocument.Parse("{" + separatedFilterAndProjection[1] + "}"),
                Projection = BsonDocument.Parse("{" + separatedFilterAndProjection[3] + "}")
            };
        }

        private struct FilterAndProjection
        {
            public BsonDocument Filter;
            public BsonDocument Projection;
        }
    }
}
