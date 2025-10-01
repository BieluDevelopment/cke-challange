using FluentResults;

using PaymentGateway.PersistantStorage.Dto;

namespace PaymentGateway.Core.Feature.Merchants.Services;

public interface IMerchantService
{
    Task<Result<Merchant>> CreateMerchantAsync();
    Task<bool> IsValidApiKeyAsync(string? apiKey);
    Task<Merchant?> GetMerchantByApiKey(string? apiKey);

}