using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;
using Dapper;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL
{
    public class SimpleDapperOrmDatabaseApi<ConnectionType> : IDatabaseApi where ConnectionType : DbConnection, new()
    {
        private readonly string _connectionString;
        private ConnectionType _connection;

        public SimpleDapperOrmDatabaseApi(string connectionString)
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


        public List<M> Search<M>(List<ISearchModel<M>> searchModel) where M : IModel, new()
        {
            List<M> result = new List<M>();
            string[] searchQueries = searchModel.Select(x => x.GetSearchString<M>()).ToArray();
            string flattenedSearchQueries = UtilityFunctions.FlattenQueries(searchQueries);

            using (var gridReader = _connection.QueryMultiple(flattenedSearchQueries))
            {
                // Grid reader uses one IEnumerable per executed query. 
                for (int i = 0; i < searchQueries.Length; i++)
                {
                    result.AddRange(gridReader.Read<M>());
                }
            }
            return result;
        }

        public int Amount<M>() where M : IModel, new()
        {
            return _connection.Query<int>($"SELECT COUNT(*) FROM {typeof(M).Name}").First();
        }


        public void Create<M>(List<M> newModels, ICreateModel createModel) where M : IModel, new()
        {
            string[] createQueries = new string[newModels.Count];
            for (int i = 0; i < newModels.Count; i++)
            {
                createQueries[i] = createModel.GetCreateString(newModels[i]);
            }

            using (var trans = _connection.BeginTransaction())
            {
                _connection.Execute(UtilityFunctions.FlattenQueries(createQueries));
                trans.Commit();
            }
        }

        public void Update<M>(List<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new()
        {
            string[] updateQueries = new string[modelsWithNewValues.Count];
            for (int i = 0; i < modelsWithNewValues.Count; i++)
            {
                updateQueries[i] = updateModel.GetUpdateString(modelsWithNewValues[i]);
            }

            using (var trans = _connection.BeginTransaction())
            {
                _connection.Execute(UtilityFunctions.FlattenQueries(updateQueries));
                trans.Commit();
            }
        }

        public void Delete<M>(List<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new()
        {
            string[] deleteQueries = new string[modelsToDelete.Count];
            for (int i = 0; i < modelsToDelete.Count; i++)
            {
                deleteQueries[i] = deleteModel.GetDeleteString(modelsToDelete[i]);
            }

            using (var trans = _connection.BeginTransaction())
            {
                _connection.Execute(UtilityFunctions.FlattenQueries(deleteQueries));
                trans.Commit();
            }
        }

        public void Truncate<M>() where M : IModel, new()
        {
            _connection.Execute($"TRUNCATE {typeof(M).Name.ToLower()};");
        }
    }
}
