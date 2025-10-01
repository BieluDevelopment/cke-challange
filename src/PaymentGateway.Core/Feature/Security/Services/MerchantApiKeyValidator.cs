using Microsoft.AspNetCore.Http;

using PaymentGateway.Core.Feature.Merchants.Services;

namespace PaymentGateway.Core.Feature.Security.Services;

public class MerchantApiKeyValidator(IMerchantService service) : IApiKeyValidator
{
    public async Task<bool> IsValidAsync(HttpContext contextHttpContext, string? apiKey)
    {
        var merchant = await service.GetMerchantByApiKey(apiKey);
        if (merchant == null)
        {
            return false;
        }
        contextHttpContext.Items.Add("merchant", merchant);
        return true;
    }
}