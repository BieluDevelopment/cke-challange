using System.Net.Http.Json;

using AwesomeAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PaymentGateway.BankIntegration.Configuration;
using PaymentGateway.BankIntegration.Models;
using PaymentGateway.BankIntegration.Services;

namespace PaymentGateway.UnitTests;

public class BankClientUnitTests
{
    [Fact]
    public async Task CanHandleException()
    {
        //Create:
        var httpClient = Substitute.For<HttpClient>();
        var optionsMonitor = Substitute.For<IOptionsMonitor<BankSimulatorClientOptions>>();
        optionsMonitor.CurrentValue.Returns(new BankSimulatorClientOptions()
        {
            Imposters = ["http://localhost:8080"]
        });
        var cancellationToken = new CancellationToken();
        var paymentRequest = new PaymentRequest
        {
            CardNumber = "2222405343248877",
            ExpiryDate = "04/2025",
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };
        var logger = new Logger<BankSimulatorClient>(new LoggerFactory());
        httpClient.PostAsJsonAsync($"{optionsMonitor.CurrentValue.Imposters.FirstOrDefault()}/payments", paymentRequest,
            cancellationToken).Throws(new InvalidOperationException());
        var client = new BankSimulatorClient(httpClient, optionsMonitor, logger);
        var result = await client.SendPayment(paymentRequest, cancellationToken);
        result.IsFailed.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
    }
}