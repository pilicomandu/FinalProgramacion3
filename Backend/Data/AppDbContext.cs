using Microsoft.EntityFrameworkCore;
using TrabajoFinalProgramacion3.Backend.Models;

namespace TrabajoFinalProgramacion3.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Models.Transaction> Transactions { get; set; }
        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .Property(t => t.CryptoAmount)
                .HasPrecision(18, 8);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Money)
                .HasPrecision(18, 2);
        }

    }
}
