using Microsoft.EntityFrameworkCore;

using NSubstitute;

using PaymentGateway.PersistantStorage;
using PaymentGateway.PersistantStorage.Extensions;

namespace PaymentGateway.UnitTests;

public abstract class BaseTest :IDisposable
{
    protected IDbContextFactory<PaymentGatewayDbContext> PrepareSubDbContextFactory(Guid guid)
    {
        var dbContextFactorySub = Substitute.For<IDbContextFactory<PaymentGatewayDbContext>>();
        EncryptionExtension.SetEncryptionKey("22ca492b4e814cab8d1b1dc4f0f560d4"); //unused key, only for testing
        dbContextFactorySub.CreateDbContextAsync().Returns(e=>
        {
            var options = new DbContextOptionsBuilder<PaymentGatewayDbContext>()
                .UseInMemoryDatabase(databaseName:$"test-{guid}")
                .Options;
            return Task.FromResult(new PaymentGatewayDbContext(options));
        });
        return dbContextFactorySub;
    }

    public void Dispose()
    {
        
    }
}