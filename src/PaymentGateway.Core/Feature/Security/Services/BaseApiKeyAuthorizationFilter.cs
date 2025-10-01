using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

using PaymentGateway.Core.Feature.Security.Constants;

namespace PaymentGateway.Core.Feature.Security.Services;

public abstract class BaseApiKeyAuthorizationFilter(IApiKeyValidator apiKeyValidator, ILogger logger) : IAsyncAuthorizationFilter
{



    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        string? apiKey = context.HttpContext.Request.Headers[SecurityConstants.ApiKeyHeaderName];

        if (!await apiKeyValidator.IsValidAsync(context.HttpContext,apiKey))
        {
            logger.LogWarning("Unauthorized access attempt with API");
            context.Result = new UnauthorizedResult();
        }
        
    }
}