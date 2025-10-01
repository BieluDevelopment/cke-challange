using Microsoft.EntityFrameworkCore;

using PaymentGateway.PersistantStorage.Dto;

namespace PaymentGateway.PersistantStorage.Services;

public class MerchantRepository(IDbContextFactory<PaymentGatewayDbContext> dbContextFactory) : IMerchantRepository
{
    public async Task<IEnumerable<Merchant?>> GetMerchants()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Merchants.ToListAsync();
    }

    public Task<Merchant?> GetMerchantAsync(Guid merchantId)
    {
        throw new NotImplementedException();
    }

    public async Task UpsertMerchantAsync(Merchant merchant)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        if (!await dbContext.Merchants.AnyAsync(x => x.Id == merchant.Id))
        {
            await dbContext.Merchants.AddAsync(merchant);
            await dbContext.SaveChangesAsync();
            return;
        }

        dbContext.Merchants.Update(merchant);
        await dbContext.SaveChangesAsync();
    }

    public Task DeleteMerchantAsync(Guid MerchantId)
    {
        throw new NotImplementedException();
    }

    public async Task<Merchant?> GetMerchantByApiKey(string? apiKey)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Merchants.FirstOrDefaultAsync(x=>x.ApiKey==apiKey);
    }
}