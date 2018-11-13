using System.Data.Entity;
using Benchmarking_Console_App.Models.DatabaseModels;

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
