﻿using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Models.DatabaseModels;
using MySql.Data.MySqlClient;
using Npgsql;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseTypes
{
    public class PostgreSQLDatabaseType : IDatabaseType
    {
        public EDatabaseType ToEnum()
        {
            return EDatabaseType.PostgreSQL;
        }

        public CrudModels<M> GetCrudModelsForDatabaseType<M>() where M : IModel, new()
        {
            return new CrudModels<M>()
            {
                CreateModel = new SqlCreateModel(),
                DeleteModel = new SqlDeleteModel(),
                GetAllModel = new SqlGetAllModel<M>(),
                SearchModel = new SqlSearchModel<M>(),
                UpdateModel = new SqlUpdateModel()
            };
        }

        public DatabaseApis GetDatabaseApi()
        {
            var connString = DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.PostgreSQL);

            return new DatabaseApis()
            {
                NormalDatabaseApi = new SimpleSQLDatabaseApi<NpgsqlCommand, NpgsqlConnection>(connString),
                ObjectOrientedDatabaseApi = null
            };
        }
    }
}
