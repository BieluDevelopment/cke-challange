using Microsoft.EntityFrameworkCore;

using PaymentGateway.Models.Entities.Responses;
using PaymentGateway.Models.Enums;
using PaymentGateway.PersistantStorage.Dto;

namespace PaymentGateway.PersistantStorage.Services;

public class PaymentsRepository(IDbContextFactory<PaymentGatewayDbContext> dbContextFactory) : IPaymentsRepository
{
    public void Dispose()
    {
        
    }

    public async Task<IEnumerable<GetPaymentResponse?>> GetPayments(Guid merchantId, int pageSize, int page)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var results= await context.Payments.Where(x=>x.MerchantId == merchantId).Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
        return results.Select(MapToDomainModel).ToList();
    }

    private GetPaymentResponse? MapToDomainModel(Payment? payment)
    {
        if (payment == null)
        {
            return null;
        }
        return new GetPaymentResponse
        {
            Amount = payment.Amount,
            Id = payment.Id,
            CardNumberLastFour = payment.CardNumber.Substring(payment.CardNumber.Length - 4,
                4),
            Currency = payment.Currency,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Status = payment.Status,
        };
    }

    public async Task<GetPaymentResponse?> GetPaymentAsync(Guid paymentID)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var result= await context.Payments.FirstOrDefaultAsync(x=>x.Id == paymentID);
        return MapToDomainModel(result);
    }

    public async Task UpsertPaymentAsync(Payment payment)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        if (!await dbContext.Payments.AnyAsync(x => x.Id == payment.Id))
        {
            await dbContext.Payments.AddAsync(payment);
            await dbContext.SaveChangesAsync();
            return;
        }

        dbContext.Payments.Update(payment);
        await dbContext.SaveChangesAsync();
    }

    public Task DeletePaymentAsync(Guid paymentID)
    {
        throw new NotImplementedException();
    }

   
}