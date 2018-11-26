using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using Benchmarking_Console_App.Configurations.Databases;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;
using Newtonsoft.Json;
using Npgsql;


namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    public class SimpleSQLDatabaseApi<CommandType, ConnectionType> : IDatabaseApi
                                                                     where CommandType : DbCommand, new()
                                                                     where ConnectionType : DbConnection, new()
    {
        private readonly string _connectionString;
        private ConnectionType _connection;

        public SimpleSQLDatabaseApi(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new ConnectionType() { ConnectionString = _connectionString };
        }


        public void OpenConnection()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }


        // TODO: OPTIMIZE
        public List<M> Search<M>(List<ISearchModel<M>> searchModels) where M : IModel, new()
        {
            var queries = searchModels.Select(x => x.GetSearchString<M>())
                                      .ToArray();

            var flattenedSearchQuery = UtilityFunctions.FlattenQueries(queries);
            return this.GetResults<M>(flattenedSearchQuery);
        }

        public int Amount<M>() where M : IModel, new()
        {
            using (var cmd = new CommandType()
            {
                CommandText = $"SELECT COUNT(*) FROM {typeof(M).Name.ToLower()};",
                Connection = _connection,
                CommandTimeout = 2000000
            })
            {
                // scalar == retrieve first row, first column only.
                return int.Parse(cmd.ExecuteScalar().ToString());
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
                using (var cmd = new CommandType()
                {
                    CommandText = createCollectionModel.GetCreateCollectionText(),
                    Connection = _connection,
                    CommandTimeout = 2000000
                })
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Create<M>(List<M> newModels, ICreateModel createModel) where M : IModel, new()
        {
            var queries = new string[newModels.Count];
            for (int i = 0; i < newModels.Count; i++)
            {
                queries[i] = createModel.GetCreateString(newModels[i]);
            }
            var flattenedCreateQueries = UtilityFunctions.FlattenQueries(queries);

            using (var trans = _connection.BeginTransaction())
            {
                var cmd = new CommandType()
                {
                    CommandText = flattenedCreateQueries,
                    Connection = _connection,
                    CommandTimeout = 2000000
                };
                cmd.ExecuteNonQuery();

                trans.Commit();
            }
        }

        public void Update<M>(List<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new()
        {
            var queries = new string[modelsWithNewValues.Count];
            for (int i = 0; i < modelsWithNewValues.Count; i++)
            {
                queries[i] = updateModel.GetUpdateString(modelsWithNewValues[i]);
            }
            var flattenedUpdateQueries = UtilityFunctions.FlattenQueries(queries);

            using (var trans = _connection.BeginTransaction())
            {
                var cmd = new CommandType()
                {
                    CommandText = flattenedUpdateQueries,
                    Connection = _connection,
                    CommandTimeout = 2000000
                };
                cmd.ExecuteNonQuery();

                trans.Commit();
            }
        }

        public void Delete<M>(List<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new()
        {
            var queries = new string[modelsToDelete.Count];
            for (int i = 0; i < modelsToDelete.Count; i++)
            {
                queries[i] = deleteModel.GetDeleteString(modelsToDelete[i]);
            }
            var flattenedUpdateQueries = UtilityFunctions.FlattenQueries(queries);

            using (var trans = _connection.BeginTransaction())
            {
                var cmd = new CommandType()
                {
                    CommandText = flattenedUpdateQueries,
                    Connection = _connection,
                    CommandTimeout = 2000000
                };
                cmd.ExecuteNonQuery();

                trans.Commit();
            }
        }

        public void Truncate<M>() where M : IModel, new()
        {
            var cmd = new CommandType() { CommandText = $"TRUNCATE {typeof(M).Name.ToLower()};", Connection = _connection, CommandTimeout = 2000000 };
            cmd.ExecuteNonQuery();

            cmd.Dispose();
        }


        private void CreateDatabase()
        {
            var connectionStringWithoutDatabasePortion = _connectionString.Replace("Database=benchmarkdb;", "");

            try
            {
                var createDbCmd = new CommandType() { CommandText = "CREATE DATABASE benchmarkdb;", Connection = _connection, CommandTimeout = 2000000 };

                createDbCmd.ExecuteNonQuery();
                createDbCmd.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine("Simple SQL API: Tried to create database benchmarkdb more than once. No further action necessary.");
            }
        }

        private List<M> GetResults<M>(string query) where M : IModel, new()
        {
            var resultingModels = new List<M>();

            using (var cmd = new CommandType() { CommandText = query, Connection = _connection, CommandTimeout = 2000000 })
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
            return resultingModels;
        }
    }
}
