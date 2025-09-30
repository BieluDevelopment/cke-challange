using Microsoft.EntityFrameworkCore;

using PaymentGateway.PersistantStorage.Converters;
using PaymentGateway.PersistantStorage.Dto;

namespace PaymentGateway.PersistantStorage;

public class PaymentGatewayDbContext : DbContext
{
    public PaymentGatewayDbContext()
    {
    }

    public PaymentGatewayDbContext(DbContextOptions options) : base(options)
    {
    }

    #region DbSets

    public DbSet<Payment> Payments { get; set; }
    public DbSet<Merchant> Merchants { get; set; }

    #endregion
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql();
        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
     var payment=   modelBuilder.Entity<Payment>();
         payment.HasKey(x => x.Id);
             payment
            .HasOne(p => p.Merchant).WithMany(x=>x.Payments)
            .HasForeignKey(p => p.MerchantId);
             payment.Property(x=>x.CardNumber).IsRequired().HasConversion<ProtectedProperty<string>>();
        payment.Property(x=>x.Cvv).IsRequired().HasConversion<ProtectedProperty<string>>();
        modelBuilder.Entity<Merchant>().HasKey(x => x.Id);
    }
}