using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;

namespace Benchmarking_Console_App.Configurations.ORMs.EntityFramework
{
    public class BenchmarkDbMySqlContextFactory : IDbContextFactory<BenchmarkDbContext>
    {
        public BenchmarkDbContext Create()
        {
            return new BenchmarkDbContext(connString: DatabaseConnectionStringFactory.GetDatabaseConnectionString(EDatabaseType.MySQL));
        }
    }
}
