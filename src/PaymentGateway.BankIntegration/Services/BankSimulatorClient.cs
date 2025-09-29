using System.Net;
using System.Net.Http.Json;

using FluentResults;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PaymentGateway.BankIntegration.Configuration;
using PaymentGateway.BankIntegration.Models;

namespace PaymentGateway.BankIntegration.Services;

public class BankSimulatorClient(HttpClient httpClient, IOptionsMonitor<BankSimulatorClientOptions> optionsMonitor, ILogger<BankSimulatorClient> logger) : IBankClient
{
    public async Task<Result<PaymentResponse>> SendPayment(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"{optionsMonitor.CurrentValue.Imposters.FirstOrDefault()}/payments", paymentRequest,
                cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseModel = await response.Content.ReadFromJsonAsync<PaymentResponse>();
            if (responseModel == null)
            {
                return Result.Fail(new Error("Payment response returned null"));
            }

            return Result.Ok(responseModel);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            return Result.Fail(new Error("Payment service unavailable"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Issue sending payment request");
            return Result.Fail(new Error("Something went wrong"));
        }
       
    }
}