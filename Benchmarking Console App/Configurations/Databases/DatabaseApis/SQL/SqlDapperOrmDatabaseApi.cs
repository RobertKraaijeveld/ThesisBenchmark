using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
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

        // TODO: OPTIMIZE
        public List<M> Search<M>(List<ISearchModel<M>> searchModel) where M : IModel, new()
        {
            List<M> result = new List<M>();
            var searchQuery = searchModel.First().GetSearchString<M>();

            result = _connection.Query<M>(searchQuery).ToList();
            return result;
        }

        public int Amount<M>() where M : IModel, new()
        {
            return _connection.Query<int>($"SELECT COUNT(*) FROM {typeof(M).Name}").First();
        }


        public void Create<M>(List<M> newModels, ICreateModel createModel) where M : IModel, new()
        {
            foreach (var model in newModels)
            {
                _connection.Execute(createModel.GetCreateString(model));
            }
        }

        public void Update<M>(List<M> modelsWithNewValues, IUpdateModel updateModel) where M : IModel, new()
        {
            foreach (var modelWithNewVals in modelsWithNewValues)
            {
                _connection.Execute(updateModel.GetUpdateString(modelWithNewVals));
            }
        }

        public void Delete<M>(List<M> modelsToDelete, IDeleteModel deleteModel) where M : IModel, new()
        {
            foreach (var model in modelsToDelete)
            {
                _connection.Execute(deleteModel.GetDeleteString(model));
            }
        }

        public void Truncate<M>() where M : IModel, new()
        {
            _connection.Execute($"TRUNCATE {typeof(M).Name.ToLower()};");
        }
    }
}
