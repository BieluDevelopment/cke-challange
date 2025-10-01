using PaymentGateway.PersistantStorage.Dto;

namespace PaymentGateway.PersistantStorage.Services;

public interface IMerchantRepository
{
    Task<IEnumerable<Merchant?>> GetMerchants();
    Task<Merchant?> GetMerchantAsync(Guid merchantId);
    Task UpsertMerchantAsync(Merchant merchant);
    Task DeleteMerchantAsync(Guid MerchantId);
    Task<Merchant?> GetMerchantByApiKey(string? apiKey);
}