using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Benchmarking_Console_App.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.ORMs.EntityFramework
{
    [DbConfigurationType(typeof(DbTypeConfiguration))]
    public class BenchmarkDbContext : DbContext
    {
        public BenchmarkDbContext(string connString) : base(connString)
        {
            Database.SetInitializer<BenchmarkDbContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Mapping uppercase modelname to lowercase tablename,
            // and specifying that EF should NOT ignore the value of the model's primary key.
            modelBuilder.Entity<MinuteAveragesRow>()
                        .ToTable("minuteaveragesrow")
                        .HasKey(m => m.MinuteAveragesRowId)
                        .Property(m => m.MinuteAveragesRowId)
                        .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

        }

        public DbSet<MinuteAveragesRow> MinuteAveragesRows { get; set; }
    }
}
