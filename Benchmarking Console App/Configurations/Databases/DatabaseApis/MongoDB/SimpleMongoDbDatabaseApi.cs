using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<M> GetAll<M>(IGetAllModel<M> getAllModel) where M : IModel, new()
        {
            var getAllQuery = getAllModel.CreateGetAllString();
            return this.GetResults<M>(getAllQuery);
        }

        public IEnumerable<M> Search<M>(ISearchModel<M> searchModel) where M : IModel, new()
        {
            var searchQuery = searchModel.GetSearchString<M>();
            return this.GetResults<M>(searchQuery);
        }

        public bool Exists<M>(M model) where M : IModel, new()
        {
            var identifiersAndValuesToSearchFor = model.GetFieldsWithValues();
            var identifiersToRetrieve = model.GetFieldsWithValues().Keys.ToList();

            var searchModel = new MongoDbSearchModel<M>(identifiersAndValuesToSearchFor, identifiersToRetrieve);

            return this.Search<M>(searchModel).Any();
        }

        public void Create<M>(IEnumerable<M> newModels, ICreateModel createModel) where M : IModel, new()
        {
            foreach (var model in newModels)
            {
                var createQueryText = createModel.GetCreateString(model);

                _database.GetCollection<M>(nameof(M))
                    .InsertOne(BsonSerializer.Deserialize<M>(createQueryText));
            }
        }

        public int Amount<M>() where M : IModel, new()
        {
            return (int) _database.GetCollection<M>(nameof(M))
                .CountDocuments(FilterDefinition<M>.Empty);
        }

        public void Update<M>(IEnumerable<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new()
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

        public void Delete<M>(IEnumerable<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new()
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


        private IEnumerable<M> GetResults<M>(string bsonFilterAndProjection) where M : IModel, new()
        {
            var results = new List<M>();

            var separateFilterAndProjection = this.GetSeparateFilterAndProjection(bsonFilterAndProjection);

            var cursor = _database.GetCollection<M>(nameof(M))
                .Find(separateFilterAndProjection.Filter)
                .Project(separateFilterAndProjection.Projection)
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
