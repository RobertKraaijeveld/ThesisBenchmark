using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Benchmarking_Console_App.Configurations.ORMs.EntityFramework;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL
{
    public class SqlOrmDatabaseApi
    {
        private readonly string _connectionString;

        public SqlOrmDatabaseApi(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<M> GetAll<M>() where M : class, IModel, new()
        {
            using (var dbContext = new BenchmarkDbContext(_connectionString))
            {
                return dbContext.Set<M>().ToList();
            }
        }

        //public IEnumerable<M> Search<M>()
        //{

        //}

        public void Create<M>(IEnumerable<M> modelsToCreate) where M : class, IModel, new()
        {
            using (var dbContext = new BenchmarkDbContext(_connectionString))
            {
                dbContext.Set<M>().AddRange(modelsToCreate);
                dbContext.SaveChanges();
            }
        }

        public void Delete<M>(IEnumerable<M> modelsToDelete) where M : class, IModel, new()
        {
            using (var dbContext = new BenchmarkDbContext(_connectionString))
            {
                dbContext.Set<M>().RemoveRange(modelsToDelete);
                dbContext.SaveChanges();
            }
        }

        public void Update<M>(IEnumerable<M> existingModelsWithNewValues) where M : class, IModel, new()
        {
            using (var dbContext = new BenchmarkDbContext(_connectionString))
            {
                // Changes to entities are detected and saved to the context automatically,
                // so we only need to call dbContext.SaveChanges(); so that the changes are persisted.
                dbContext.SaveChanges();
            }
        }

        public void Truncate<M>() where M : class, IModel, new()
        {
            using (var dbContext = new BenchmarkDbContext(_connectionString))
            {
                var set = dbContext.Set<M>();
                var allEntitiesOfSet = set.ToList();
                set.RemoveRange(allEntitiesOfSet);
            }
        }
    }
}
