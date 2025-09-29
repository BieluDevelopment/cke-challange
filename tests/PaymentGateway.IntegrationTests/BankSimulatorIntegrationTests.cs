using System.Net.Http.Json;

using AwesomeAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using PaymentGateway.BankIntegration.Configuration;
using PaymentGateway.BankIntegration.Models;
using PaymentGateway.BankIntegration.Services;

using Projects;

namespace PaymentGateway.IntegrationTests;

public class BankSimulatorIntegrationTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
   
    [Fact]
    public async Task ReturnsValidAuthCode()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;
        var appHost =
            await DistributedApplicationTestingBuilder
                .CreateAsync<PaymentGateway_AppHost>(cancellationToken);
     
        appHost.Services.Configure<BankSimulatorClientOptions>(e =>
        {
            e.Imposters = new List<string>() { "http://localhost:8080" };
        });

        appHost.Services.AddHttpClient<IBankClient, BankSimulatorClient>();
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
        var client = app.Services.GetService<IBankClient>();
        var response = await client.SendPayment(new PaymentRequest
        {
            CardNumber = "2222405343248877",
            ExpiryDate = "04/2025",
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        });
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Value.AuthorizationCode.Should().NotBeEmpty();
        response.Value.Authorized.Should().BeTrue();
        // Assert
    }
    [Fact]
    public async Task ReturnsInvalidAuthCode()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;
        var appHost =
            await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.PaymentGateway_AppHost>(cancellationToken);
       
        appHost.Services.Configure<BankSimulatorClientOptions>(e =>
        {
            e.Imposters = new List<string>() { "http://localhost:8080" };
        });

        appHost.Services.AddHttpClient<IBankClient, BankSimulatorClient>();
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
        var client = app.Services.GetService<IBankClient>();
        var response = await client.SendPayment(new PaymentRequest
        {
            CardNumber = "2222405343248872",
            ExpiryDate = "04/2025",
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        });
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Value.AuthorizationCode.Should().BeEmpty();
        response.Value.Authorized.Should().BeFalse();
        // Assert
    }
     [Fact]
    public async Task ReturnsServiceUnavailable()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;
        var appHost =
            await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.PaymentGateway_AppHost>(cancellationToken);
  
        appHost.Services.Configure<BankSimulatorClientOptions>(e =>
        {
            e.Imposters = new List<string>() { "http://localhost:8080" };
        });

        appHost.Services.AddHttpClient<IBankClient, BankSimulatorClient>();
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
        var client = app.Services.GetService<IBankClient>();
        var response = await client.SendPayment(new PaymentRequest
        {
            CardNumber = "2222405343248870",
            ExpiryDate = "04/2025",
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        });
        response.IsFailed.Should().BeTrue();
        response.IsSuccess.Should().BeFalse();
        // Assert
    }
}