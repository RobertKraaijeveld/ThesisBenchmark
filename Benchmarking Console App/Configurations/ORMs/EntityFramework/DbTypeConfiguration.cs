using System;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using Npgsql;


namespace Benchmarking_Console_App.Configurations.ORMs.EntityFramework
{
    public class DbTypeConfiguration : DbConfiguration
    {
        private readonly string DatabaseForOrmConfigFileLocation = "C:\\Projects\\Afstudeerexperimenten\\Benchmarking Console App\\Benchmarking Console App\\database_for_orm.config";

        public DbTypeConfiguration()
        {
            var databaseType = DbTypeWhichWillBeUsedWithEF();

            switch (databaseType)
            {
                case EDatabaseType.MySQL:
                {
                    SetProviderFactory("MySql.Data.MySqlClient", MySqlClientFactory.Instance);
                    SetProviderServices("System.Data.SqlClient", SqlProviderServices.Instance);
                    SetContextFactory(() => new BenchmarkDbMySqlContextFactory().Create());
                    SetDefaultConnectionFactory(new MySqlConnectionFactory());

                    break;
                }
                case EDatabaseType.PostgreSQL:
                {
                    SetProviderFactory("Npgsql", NpgsqlFactory.Instance);
                    SetProviderServices("Npgsql", NpgsqlServices.Instance);
                    SetContextFactory(() => new BenchmarkDbPostgreSQLContextFactory().Create());
                    SetDefaultConnectionFactory(new NpgsqlConnectionFactory());

                    break;
                }
                default:
                    throw new Exception("Entity Framework can only be used with MySQL or PostgreSQL");
            }
        }

        private EDatabaseType DbTypeWhichWillBeUsedWithEF()
        {
            var firstLineOfFile = File.ReadAllLines(DatabaseForOrmConfigFileLocation)
                                      .First();

            EDatabaseType outResult;

            EDatabaseType.TryParse(firstLineOfFile, out outResult);
            return outResult;
        }
    }
}
