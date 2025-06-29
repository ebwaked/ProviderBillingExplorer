using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace ProviderBilling.Data
{
    public class ProviderBillingContext : DbContext
    {
        public DbSet<Provider> Providers { get; set; }
        public DbSet<BillingRecord> BillingRecords { get; set; }

        public ProviderBillingContext(DbContextOptions<ProviderBillingContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Provider>()
                .ToTable("Provider")
                .HasKey(p => p.NPI);

            modelBuilder.Entity<BillingRecord>()
                .ToTable("BillingRecord")
                .HasOne(b => b.Provider)
                .WithMany(p => p.BillingRecords)
                .HasForeignKey(b => b.NPI);
        }
    }
}
