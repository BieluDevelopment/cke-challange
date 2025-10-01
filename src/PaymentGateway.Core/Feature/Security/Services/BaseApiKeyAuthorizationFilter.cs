using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using PaymentGateway.Core.Feature.Security.Constants;

namespace PaymentGateway.Core.Feature.Security.Services;

public abstract class BaseApiKeyAuthorizationFilter(IApiKeyValidator apiKeyValidator) : IAsyncAuthorizationFilter
{



    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        string? apiKey = context.HttpContext.Request.Headers[SecurityConstants.ApiKeyHeaderName];

        if (!await apiKeyValidator.IsValidAsync(context.HttpContext,apiKey))
        {
            context.Result = new UnauthorizedResult();
        }
        
    }
}