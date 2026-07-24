using Microsoft.EntityFrameworkCore;
using TimescaleApi.Models;

namespace TimescaleApi.Data;

public class AppDbContext : DbContext
{
    public DbSet<ValueRecord> Values { get; set; }
    public DbSet<ResultRecord> Results { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ValueRecord>(entity =>
        {
            entity.ToTable("Values");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.ExecutionTime).IsRequired();
            entity.Property(e => e.Value).IsRequired();
            
            entity.HasIndex(e => e.FileName);
            entity.HasIndex(e => new {e.FileName, e.Date });
        });

        modelBuilder.Entity<ResultRecord>(entity =>
        {
            entity.ToTable("Results");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.AvgExecutionTime).IsRequired();
            entity.Property(e => e.AvgValue).IsRequired();
            entity.Property(e => e.MedianValue).IsRequired();
            entity.Property(e => e.MaxValue).IsRequired();
            entity.Property(e => e.MinValue).IsRequired();

            entity.HasIndex(e => e.FileName).IsUnique();
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.AvgValue);
            entity.HasIndex(e => e.AvgExecutionTime);
        });
    }
}