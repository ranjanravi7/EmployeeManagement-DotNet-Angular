using Microsoft.EntityFrameworkCore;
using DbFirstCore.DataAccessLayer.Models;

namespace DbFirstCore.DataAccessLayer
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>(b =>
            {
                // Map to existing database table named "Employee" (singular) if the DB uses that name.
                // By default EF Core pluralizes entity names to table names (Employees). Use ToTable to match DB.
                b.ToTable("Employee");
                b.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
                b.Property(e => e.LastName).HasMaxLength(100).IsRequired();
                b.Property(e => e.Email).HasMaxLength(256);
                b.Property(e => e.Salary).HasColumnType("decimal(18,2)");
                b.Property(e => e.RowVersion).IsRowVersion();
            });
        }
    }
}
