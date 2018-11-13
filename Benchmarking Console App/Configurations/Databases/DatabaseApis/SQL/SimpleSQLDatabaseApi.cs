using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
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
        // Used to store table names that have been filled, these tablenames are used when calling TruncateAll().
        // Weak solution since tables might be filled without being touched by this API.
        private static HashSet<string> NamesOfFilledTables = new HashSet<string>();
        private readonly string _connectionString;

        public SimpleSQLDatabaseApi(string connectionString)
        {
            _connectionString = connectionString;
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

        public int Amount<M>() where M : IModel, new()
        {
            using (var conn = new ConnectionType() {ConnectionString = _connectionString})
            {
                conn.Open();
                using (var cmd = new CommandType() { CommandText = $"SELECT COUNT(*) FROM {typeof(M).Name.ToLower()};",
                                                     Connection = conn })
                {
                    // scalar == retrieve first row, first column only.
                    return int.Parse(cmd.ExecuteScalar().ToString());
                }
            }
        }

        public void CreateCollectionIfNotExists<M>(ICreateCollectionModel<M> createCollectionModel) where M : IModel, new()
        {
            try
            {
                //Trying to get the amount of models of this type. Will throw an exception if the table doesn't exist
                var amountOfModelsOfType = this.Amount<M>();
            }
            catch (Exception e) // table does not exist so we create it
            {
                using (var conn = new ConnectionType() { ConnectionString = _connectionString })
                {
                    conn.Open();
                    using (var cmd = new CommandType() { CommandText = createCollectionModel.GetCreateCollectionText(),
                                                         Connection = conn })
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
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
                    var cmd = new CommandType() { CommandText = deleteModel.GetDeleteString(model),
                                                  Connection = conn };
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void TruncateAll()
        {
            using (var conn = new ConnectionType() {ConnectionString = _connectionString})
            {
                conn.Open();

                foreach (var table in NamesOfFilledTables)
                {
                    var cmd = new CommandType() { CommandText = $"TRUNCATE {table};", Connection = conn };
                    cmd.ExecuteNonQuery();

                    cmd.Dispose();
                }
            }
        }

        // TODO: Temporary fix for SQL Truncation problem of not knowing table names in advance
        public void Truncate<M>() where M: IModel, new()
        {
            using (var conn = new ConnectionType() {ConnectionString = _connectionString})
            {
                conn.Open();

                var cmd = new CommandType() {CommandText = $"TRUNCATE {typeof(M).Name.ToLower()};", Connection = conn};
                cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
        }

        private IEnumerable<M> GetResults<M>(string query) where M: IModel, new()
        {
            var resultingModels = new List<M>();

            using (var conn = new ConnectionType() { ConnectionString = _connectionString })
            {
                conn.Open();

                using (var cmd = new CommandType() { CommandText =query, Connection = conn })
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

                        resultingModels.Add(rowAsModel);
                    }
                }
            }
            return resultingModels;
        }
    }
}
