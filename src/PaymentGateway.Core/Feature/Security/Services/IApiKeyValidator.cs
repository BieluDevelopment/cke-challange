using Microsoft.AspNetCore.Http;

namespace PaymentGateway.Core.Feature.Security.Services;

public interface IApiKeyValidator
{
    Task<bool> IsValidAsync(HttpContext contextHttpContext, string? apiKey);
}