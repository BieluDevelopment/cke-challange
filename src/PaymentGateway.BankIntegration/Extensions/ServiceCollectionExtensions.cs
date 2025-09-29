using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PaymentGateway.BankIntegration.Configuration;
using PaymentGateway.BankIntegration.Services;

namespace PaymentGateway.BankIntegration.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddBankClient(this IHostApplicationBuilder builder, Action<IHttpClientBuilder>? configure = null)
    {
        builder.Services.Configure<BankSimulatorClientOptions>(builder.Configuration.GetSection(BankSimulatorClientOptions.SectionName));

        var httpClientBuilder = builder.Services.AddHttpClient<IBankClient, BankSimulatorClient>();
        configure?.Invoke(httpClientBuilder);
        return builder;
    }
}