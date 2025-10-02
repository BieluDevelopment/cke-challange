using System.Net.Http.Json;

using AwesomeAssertions;

using Microsoft.Extensions.Logging;

using PaymentGateway.BankIntegration.Models;
using PaymentGateway.Models.Entities.Requests;
using PaymentGateway.Models.Entities.Responses;
using PaymentGateway.Models.Enums;
using PaymentGateway.PersistantStorage.Dto;

using Projects;

namespace PaymentGateway.IntegrationTests;

public class PaymentsIntegrationTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(2);

    [Fact]
    public async Task CanProcessSuccessfulPayment()
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
         var client = app.CreateHttpClient("payment-gateway");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/merchant/create");
        httpRequest.Headers.Add("x-api-key", "anyApiKey");
        var response = await client.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Merchant>();
        if (result == null)
        {
            throw new Exception("Merchant result is null");
        }

        var paymentRequest = new MerchantPaymentProcessRequest
        {
            CardNumber = "2222405343248871",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Name = "Test User",
            MerchantId = result.Id
        };
        var httpRequestForPayment =
            new HttpRequestMessage(HttpMethod.Post, "/api/payment/process");
        httpRequestForPayment.Content = JsonContent.Create(paymentRequest);
        httpRequestForPayment.Headers.Add("x-api-key", result.ApiKey);
        var paymentStatus = await client.SendAsync(httpRequestForPayment, cancellationToken);
        paymentStatus.EnsureSuccessStatusCode();
        var paymentResult =
            await paymentStatus.Content.ReadFromJsonAsync<PostPaymentResponse>(cancellationToken: cancellationToken);
        // Assert
        paymentResult.Should().NotBeNull();
        paymentResult.Status.Should().Be(PaymentStatus.Authorized);
        paymentResult.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CanProcessDeclinedPayment()
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
         var client = app.CreateHttpClient("payment-gateway");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/merchant/create");
        httpRequest.Headers.Add("x-api-key", "anyApiKey");
        var response = await client.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Merchant>();
        if (result == null)
        {
            throw new Exception("Merchant result is null");
        }

        var paymentRequest = new MerchantPaymentProcessRequest
        {
            CardNumber = "2222405343248872",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Name = "Test User",
            MerchantId = result.Id
        };
        var httpRequestForPayment =
            new HttpRequestMessage(HttpMethod.Post, "/api/payment/process");
        httpRequestForPayment.Content = JsonContent.Create(paymentRequest);
        httpRequestForPayment.Headers.Add("x-api-key", result.ApiKey);
        var paymentStatus = await client.SendAsync(httpRequestForPayment, cancellationToken);
        var paymentResult =
            await paymentStatus.Content.ReadFromJsonAsync<PostPaymentResponse>(cancellationToken: cancellationToken);
        paymentStatus.EnsureSuccessStatusCode();
 
        // Assert
        paymentResult.Should().NotBeNull();
        paymentResult.Status.Should().Be(PaymentStatus.Declined);
        paymentResult.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CanGetPaymentAfterProcessSuccessfulPayment()
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
        var client = app.CreateHttpClient("payment-gateway");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/merchant/create");
        httpRequest.Headers.Add("x-api-key", "anyApiKey");
        var response = await client.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Merchant>();
        if (result == null)
        {
            throw new Exception("Merchant result is null");
        }

        var paymentRequest = new MerchantPaymentProcessRequest
        {
            CardNumber = "2222405343248871",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Name = "Test User",
            MerchantId = result.Id
        };
        var httpRequestForPayment =
            new HttpRequestMessage(HttpMethod.Post, "/api/payment/process");
        httpRequestForPayment.Content = JsonContent.Create(paymentRequest);
        httpRequestForPayment.Headers.Add("x-api-key", result.ApiKey);
        var paymentStatus = await client.SendAsync(httpRequestForPayment, cancellationToken);
        paymentStatus.EnsureSuccessStatusCode();
        var paymentResult =
            await paymentStatus.Content.ReadFromJsonAsync<PostPaymentResponse>(cancellationToken: cancellationToken);
        var getPaymentRequest =
            new HttpRequestMessage(HttpMethod.Get, $"/api/payment/{paymentResult.Id}");
        getPaymentRequest.Headers.Add("x-api-key", result.ApiKey);
        var paymentDetailsResponse = await client.SendAsync(getPaymentRequest, cancellationToken);
        var paymentDetails =
            await paymentDetailsResponse.Content.ReadFromJsonAsync<GetPaymentResponse>(cancellationToken: cancellationToken);
        paymentDetailsResponse.EnsureSuccessStatusCode();

   

        // Assert
        paymentDetails.Should().NotBeNull();
        paymentDetails.Status.Should().Be(PaymentStatus.Authorized);
        paymentDetails.Id.Should().NotBeEmpty();
    }
}