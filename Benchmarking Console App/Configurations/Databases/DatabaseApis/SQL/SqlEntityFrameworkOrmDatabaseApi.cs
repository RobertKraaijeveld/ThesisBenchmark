﻿using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarking_Console_App.Configurations.ORMs.EntityFramework;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL
{
    public class SqlEntityFrameworkOrmDatabaseApi 
    {
        private readonly string _connectionString;

        public SqlEntityFrameworkOrmDatabaseApi(string connectionString)
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

        public IEnumerable<M> Search<M>(Func<M, bool> predicate) where M : class, IModel, new()
        {
            using (var dbContext = new BenchmarkDbContext(_connectionString))
            {
                return dbContext.Set<M>().Where(predicate).ToList();
            }
        }

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
                modelsToDelete.ToList().ForEach(x => dbContext.Set<M>().Attach(x));

                dbContext.Set<M>().RemoveRange(modelsToDelete);
                dbContext.SaveChanges();
            }
        }

        public void Update<M>(IEnumerable<M> existingModelsWithNewValues) where M : class, IModel, new()
        {
            using (var dbContext = new BenchmarkDbContext(_connectionString))
            {
                existingModelsWithNewValues.ToList().ForEach(x => dbContext.Set<M>().Attach(x));
                dbContext.SaveChanges();
            }
        }

        public void Truncate<M>() where M : class, IModel, new()
        {
            using (var dbContext = new BenchmarkDbContext(_connectionString))
            {
                dbContext.Database.ExecuteSqlCommand($"TRUNCATE TABLE {typeof(M).Name.ToLower()}");
            }
        }
    }
}
