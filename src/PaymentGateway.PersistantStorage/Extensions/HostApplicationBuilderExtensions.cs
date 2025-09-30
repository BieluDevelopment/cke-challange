
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PaymentGateway.PersistantStorage.Services;

namespace PaymentGateway.PersistantStorage.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddPersistentStorage(this IHostApplicationBuilder builder)
    {
        var localConnectionString = builder.Configuration.GetConnectionString("postgressDb");
        builder.AddNpgsqlDbContext<PaymentGatewayDbContext>("postgressDb");

        builder.Services.AddDbContextFactory<PaymentGatewayDbContext>(( options) =>
        {
            options.UseNpgsql(localConnectionString);
        }, ServiceLifetime.Scoped);
        builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();

        return builder;
    }
}