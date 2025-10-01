using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PaymentGateway.BankIntegration.Configuration;
using PaymentGateway.BankIntegration.Services;

namespace PaymentGateway.BankIntegration.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddBankClient(this IHostApplicationBuilder builder, Action<IHttpClientBuilder>? configure = null)
    {
        //services__bankSimulator__imposters__0
        var section = builder.Configuration.GetSection(BankSimulatorClientOptions.SectionName);
        if (string.IsNullOrEmpty(section?.Value))
        {
            section = builder.Configuration.GetSection(BankSimulatorClientOptions.SectionName.Replace("__",":"));
        }
        builder.Services.Configure<BankSimulatorClientOptions>(section);

        var httpClientBuilder = builder.Services.AddHttpClient<IBankClient, BankSimulatorClient>();
        configure?.Invoke(httpClientBuilder);
        return builder;
    }
}