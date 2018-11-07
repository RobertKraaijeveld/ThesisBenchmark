using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;
using Newtonsoft.Json;




namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    /// <summary>
    /// Provides basic CRUD functionality for SQL databases.
    /// A connection string must be provided, as well as two type parameters:
    /// - CommandType: A concrete implementation of the ADO.NET DbCommand class.
    /// - ConnectionType: A concrete implementation of the ADO.NET DbConnection class. 
    /// </summary>
    public class SimpleSQLDatabaseApi<CommandType, ConnectionType> : IDatabaseApi
                                                               where CommandType: DbCommand, new()
                                                               where ConnectionType: DbConnection, new()
    {
        private readonly string _connectionString;

        public SimpleSQLDatabaseApi(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<M> Get<M>(string collectionName, ISearchModel searchModel) where M : IModel, new()  
        {
            var resultingModels = new List<M>();

            using (var conn = new ConnectionType() { ConnectionString = _connectionString})
            {
                conn.Open();

                using (var cmd = new CommandType() { CommandText = searchModel.GetSearchString(collectionName), Connection = conn})
                using (var reader = cmd.ExecuteReader())
                {
                    // Getting column names and types, making sure all col names are lowercase, comparison will also be lowercase
                    var columnNamesAndTypes = Enumerable.Range(0, reader.FieldCount)
                                                        .ToDictionary(reader.GetName, reader.GetFieldType)
                                                        .ToDictionary(key => key.Key.ToLower(), value => value.Value); 

                    var columnNames = columnNamesAndTypes.Keys.ToList();

                    while (reader.Read())
                    {
                        var values = new object[reader.FieldCount];
                        reader.GetValues(values);

                        var rowAsDict = columnNames.Zip(values, (c, v) => new { column = c, value = v })
                                                   .ToDictionary(key => key.column, value => value.value);

                        var rowAsJson = JsonConvert.SerializeObject(rowAsDict);
                        var rowAsModel = JsonConvert.DeserializeObject<M>(rowAsJson); 

                        if (rowAsModel.SchemaMatches(columnNamesAndTypes)) resultingModels.Add(rowAsModel);
                        else throw new Exception("Model schema and row schema do not match");
                    }
                }
            }
            return resultingModels;
        }

        public void Create<M>(IEnumerable<M> newModels, ICreateModel createModel) where M : IModel, new()
        {
            using (var conn = new ConnectionType() { ConnectionString = _connectionString })
            {
                conn.Open();

                foreach(var model in newModels)
                {
                    using (var cmd = new CommandType() {CommandText = createModel.GetCreateString(model),
                                                        Connection = conn})
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void Update<M>(IEnumerable<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new()
        {
            using (var conn = new ConnectionType() { ConnectionString = _connectionString })
            {
                conn.Open();

                foreach (var modelWithNewVals in modelsWithNewValues)
                {
                    var cmd = new CommandType() { CommandText = updateModel.GetUpdateString(modelWithNewVals),
                                                  Connection = conn };
                    cmd.ExecuteNonQuery();
                }
                
            }
        }

        public void Delete<M>(IEnumerable<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new()
        {
            using (var conn = new ConnectionType() { ConnectionString = _connectionString })
            {
                conn.Open();

                foreach (var model in modelsToDelete)
                {
                    var propertiesAndValuesOfModel = model.GetFieldsWithValues();
                   
                    var cmd = new CommandType() { CommandText = deleteModel.GetDeleteString(model),
                                                  Connection = conn };
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
