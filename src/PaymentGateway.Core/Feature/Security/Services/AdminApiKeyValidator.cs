using Microsoft.AspNetCore.Http;

namespace PaymentGateway.Core.Feature.Security.Services;

public class AdminApiKeyValidator() : IApiKeyValidator
{
    public Task<bool> IsValidAsync(HttpContext contextHttpContext, string? apiKey)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(apiKey)); // this is just a example implementation
    }
}