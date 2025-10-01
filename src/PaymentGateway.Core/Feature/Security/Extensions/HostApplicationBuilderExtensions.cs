using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PaymentGateway.Core.Feature.Security.Services;

namespace PaymentGateway.Core.Feature.Security.Extensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddSecurity(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<MerchantApiKeyAuthorizationFilter>();
        builder.Services.AddScoped<AdminApiKeyAuthorizationFilter>();

        builder.Services.AddKeyedScoped<IApiKeyValidator, MerchantApiKeyValidator>("merchant");
        builder.Services.AddKeyedScoped<IApiKeyValidator, AdminApiKeyValidator>("admin");
        return builder;
    }

}