using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL
{
    public class SimpleDapperOrmDatabaseApi<CommandType, ConnectionType> : IDatabaseApi
                                                                           where CommandType : DbCommand, new()
                                                                           where ConnectionType : DbConnection, new()
    {
        private readonly string _connectionString;

        public SimpleDapperOrmDatabaseApi(string connectionString)
        {
            _connectionString = connectionString;
        }


        public IEnumerable<M> GetAll<M>(IGetAllModel<M> getAllModel) where M : IModel, new()
        {
            IEnumerable<M> result = new List<M>();
            var getAllQuery = getAllModel.CreateGetAllString();

            using (var conn = new ConnectionType() {ConnectionString = _connectionString})
            {
                conn.Open();

                result = conn.Query<M>(getAllQuery);
            }
            return result;
        }

        public IEnumerable<M> Search<M>(ISearchModel<M> searchModel) where M : IModel, new()
        {
            IEnumerable<M> result = new List<M>();
            var searchQuery = searchModel.GetSearchString<M>();

            using (var conn = new ConnectionType() { ConnectionString = _connectionString })
            {
                conn.Open();

                result = conn.Query<M>(searchQuery);
            }
            return result;
        }

        public int Amount<M>() where M : IModel, new()
        {
            using (var conn = new ConnectionType() {ConnectionString = _connectionString})
            {
                conn.Open();
                return conn.Query<int>($"SELECT COUNT(*) FROM {typeof(M).Name}").First();
            }
        }

        public void Create<M>(IEnumerable<M> newModels, ICreateModel createModel) where M : IModel, new()
        {
            using (var conn = new ConnectionType() { ConnectionString = _connectionString })
            {
                conn.Open();

                foreach (var model in newModels)
                {
                    conn.Execute(createModel.GetCreateString(model));
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
                    conn.Execute(updateModel.GetUpdateString(modelWithNewVals));
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
                    conn.Execute(deleteModel.GetDeleteString(model));
                }
            }
        }

        public void Truncate<M>() where M : IModel, new()
        {
            using (var conn = new ConnectionType() { ConnectionString = _connectionString })
            {
                conn.Open();

                conn.Execute($"TRUNCATE {typeof(M).Name.ToLower()};");
            }
        }
    }
}
