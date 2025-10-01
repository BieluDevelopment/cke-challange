using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PaymentGateway.Core.Feature.Security.Services;

public class MerchantApiKeyAuthorizationFilter([FromKeyedServices("merchant")]IApiKeyValidator apiKeyValidator, ILogger<AdminApiKeyAuthorizationFilter> logger) : BaseApiKeyAuthorizationFilter(apiKeyValidator,logger)
{
    
}