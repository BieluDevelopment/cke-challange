using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

using PaymentGateway.Core.Feature.Security.Constants;

namespace PaymentGateway.Core.Feature.Security.Services;

public class ApiKeySchemeTransformer() : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
       
            // Add the security scheme at the document level
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["api_key"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "api_key", 
                    Description = "Api key provided by admin, unique for each of merchants",
                    In = ParameterLocation.Header,
                    Name = SecurityConstants.ApiKeyHeaderName,
                    BearerFormat = Guid.Empty.ToString().Replace("-", string.Empty),
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;

            // Apply it as a requirement for all operations
            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
            
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id ="api_key", Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                });
            }
    }
}