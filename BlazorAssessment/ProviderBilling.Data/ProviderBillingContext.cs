using Common.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
            .HasKey(p => p.NPI);

        modelBuilder.Entity<BillingRecord>()
            .HasOne(b => b.Provider)
            .WithMany(p => p.BillingRecords)
            .HasForeignKey(b => b.NPI);
    }
}
}
