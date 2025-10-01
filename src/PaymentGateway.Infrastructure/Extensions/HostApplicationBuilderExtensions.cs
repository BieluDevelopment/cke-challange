using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PaymentGateway.BankIntegration.Extensions;
using PaymentGateway.PersistantStorage.Extensions;

using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;

namespace PaymentGateway.Infrastructure.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddBankClient(SetupHttpClientWithResilience);
        builder.AddPersistentStorage();
        return builder;
    }

    private static void SetupHttpClientWithResilience(IHttpClientBuilder httpClientBuilder)
    {
        var retryPolicy = GetRetryPolicy();
        var circuitBreakerPolicy = GetCircuitBreakerPolicy();
        httpClientBuilder.SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Sample: default lifetime is 2 minutes
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(circuitBreakerPolicy);
    }

    static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                retryAttempt)));
    }

    static AsyncCircuitBreakerPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}