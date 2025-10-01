using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PaymentGateway.Core.Feature.Merchants.Services;
using PaymentGateway.Core.Feature.Payments.Services;

namespace PaymentGateway.Core.Feature.Merchants.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddMerchants(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IMerchantService, MerchantService>();
        return builder;
    }

}