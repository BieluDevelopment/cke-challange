using Microsoft.EntityFrameworkCore;

using PaymentGateway.Models.Entities.Responses;
using PaymentGateway.Models.Enums;
using PaymentGateway.PersistantStorage.Dto;

namespace PaymentGateway.PersistantStorage.Services;

public class PaymentsRepository(IDbContextFactory<PaymentGatewayDbContext> dbContext) : IPaymentsRepository, IAsyncDisposable
{
    public void Dispose()
    {
        
    }

    public async Task<IEnumerable<GetPaymentResponse?>> GetPayments(Guid merchantId, int pageSize, int page)
    {
        using var context = dbContext.CreateDbContext();
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
        using var context = dbContext.CreateDbContext();
        var result= await context.Payments.FirstOrDefaultAsync(x=>x.Id == paymentID);
        return MapToDomainModel(result);
    }

    public Task UpsertPaymentAsync(Payment payment)
    {
        throw new NotImplementedException();
    }

    public Task DeletePaymentAsync(Guid paymentID)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}