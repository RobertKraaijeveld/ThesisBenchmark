using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benchmarking_Console_App.Models.DatabaseModels;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.ORMs.EntityFramework
{
    [DbConfigurationType(typeof(DbTypeConfiguration))]
    public class BenchmarkDbContext : DbContext
    {
        public BenchmarkDbContext(string connString) : base(connString)
        {
        }

        //Write Fluent API configurations here
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MinuteAveragesRow>()
                        .ToTable("minuteaveragesrow");
        }



        public DbSet<MinuteAveragesRow> MinuteAveragesRows { get; set; }
    }
}
