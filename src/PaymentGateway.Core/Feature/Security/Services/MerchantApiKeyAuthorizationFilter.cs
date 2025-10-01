using Microsoft.Extensions.DependencyInjection;

namespace PaymentGateway.Core.Feature.Security.Services;

public class MerchantApiKeyAuthorizationFilter([FromKeyedServices("merchant")]IApiKeyValidator apiKeyValidator) : BaseApiKeyAuthorizationFilter(apiKeyValidator)
{
    
}