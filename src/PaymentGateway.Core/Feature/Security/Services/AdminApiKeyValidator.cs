using Microsoft.AspNetCore.Http;

namespace PaymentGateway.Core.Feature.Security.Services;

public class AdminApiKeyValidator() : IApiKeyValidator
{
    public Task<bool> IsValidAsync(HttpContext contextHttpContext, string? apiKey)
    {
        return Task.FromResult(true); //todo make it read from Iconfiguration
    }
}