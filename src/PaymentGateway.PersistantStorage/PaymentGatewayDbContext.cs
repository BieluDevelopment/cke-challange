using Microsoft.EntityFrameworkCore;

namespace PaymentGateway.PersistantStorage;

public class PaymentGatewayDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    
    }
}