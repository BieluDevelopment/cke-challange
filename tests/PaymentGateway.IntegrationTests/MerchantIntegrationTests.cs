using System.Net.Http.Json;

using AwesomeAssertions;

using Microsoft.Extensions.Logging;

using PaymentGateway.BankIntegration.Models;
using PaymentGateway.BankIntegration.Services;
using PaymentGateway.PersistantStorage.Dto;

using Projects;

namespace PaymentGateway.IntegrationTests;

public class MerchantIntegrationTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    
    [Fact]
    public async Task CanCreateMerchant()
    {
        var cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;
        var appHost =
            await DistributedApplicationTestingBuilder
                .CreateAsync<PaymentGateway_AppHost>(cancellationToken);
     
        appHost.Services.AddHttpClient();
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
            // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        await app.ResourceNotifications.WaitForResourceHealthyAsync("bankSimulator", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        await app.ResourceNotifications.WaitForResourceHealthyAsync("payment-gateway", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        var client = app.Services.GetService<HttpClient>();
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7296/api/merchant/create");
        var response = await client.SendAsync(httpRequest);
      
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Merchant>();
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.ApiKey.Should().NotBeEmpty();
    }
}