
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PaymentGateway.PersistantStorage.Services;

namespace PaymentGateway.PersistantStorage.Extensions;

public static class HostApplicationBuilderExtensions
{
    private static IHostApplicationBuilder AddServices(this IHostApplicationBuilder builder)
    {
        var encryptionKey = builder.Configuration.GetValue<string>("DbSettings:EncryptionKey");
        
        EncryptionExtension.SetEncryptionKey(encryptionKey);
        builder.Services.AddScoped<IPaymentsRepository, PaymentsRepository>();
        builder.Services.AddScoped<IMerchantRepository, MerchantRepository>();

        return builder;
    }
    public static IHostApplicationBuilder AddPersistentStorage(this IHostApplicationBuilder builder)
    {
        var localConnectionString = builder.Configuration.GetConnectionString("postgressDb");
        builder.AddNpgsqlDbContext<PaymentGatewayDbContext>("postgressDb");

        builder.Services.AddDbContextFactory<PaymentGatewayDbContext>(( options) =>
        {
            options.UseNpgsql(localConnectionString, e =>
            {
                e.EnableRetryOnFailure(
                    maxRetryCount: 2,
                    maxRetryDelay: TimeSpan.FromSeconds(30), null);
            });
           
          
        });
        builder.AddServices();

        return builder;
    }
    public static IHostApplicationBuilder AddInMemoryStorage(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContextFactory<PaymentGatewayDbContext>(( options) =>
        {
            options.UseInMemoryDatabase("testDb");
            
        });
     
        builder.AddServices();
        return builder;
    }
}