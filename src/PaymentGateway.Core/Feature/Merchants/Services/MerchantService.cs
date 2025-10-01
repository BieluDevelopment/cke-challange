using FluentResults;

using PaymentGateway.PersistantStorage.Dto;
using PaymentGateway.PersistantStorage.Services;

namespace PaymentGateway.Core.Feature.Merchants.Services;

public class MerchantService(IMerchantRepository merchantRepository) : IMerchantService
{
    public async Task<Result<Merchant>> CreateMerchantAsync()
    {
        var merchant = new Merchant()
        {
            Id = Guid.NewGuid(), ApiKey = Guid.NewGuid().ToString().Replace("-", string.Empty),
        };
        await merchantRepository.UpsertMerchantAsync(merchant);
        return merchant;
    }

    public async Task<bool> IsValidApiKeyAsync(string? apiKey)
    {
        var merchants = await merchantRepository.GetMerchantByApiKey(apiKey);
        return merchants!= null;
    }
    public async Task<Merchant?> GetMerchantByApiKey(string? apiKey)
    {
        return await merchantRepository.GetMerchantByApiKey(apiKey);
    }
}