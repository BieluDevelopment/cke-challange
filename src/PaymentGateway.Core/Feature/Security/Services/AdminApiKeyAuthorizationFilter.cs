using Microsoft.Extensions.DependencyInjection;

namespace PaymentGateway.Core.Feature.Security.Services;

public class AdminApiKeyAuthorizationFilter([FromKeyedServices("admin")]IApiKeyValidator apiKeyValidator) : BaseApiKeyAuthorizationFilter(apiKeyValidator)
{
    
}