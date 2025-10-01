using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Core.Feature.Security.Services;

namespace PaymentGateway.Core.Feature.Security.Attributes;

public class ValidateAdminApiKeyAttribute() : ServiceFilterAttribute(typeof(AdminApiKeyAuthorizationFilter))
{
    
}