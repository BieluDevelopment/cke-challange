using System.Runtime.CompilerServices;

using FluentResults;

using Microsoft.Extensions.Caching.Memory;

using PaymentGateway.Models.Entities.Requests;
using PaymentGateway.Models.Entities.Responses;
[assembly: InternalsVisibleTo("PaymentGateway.UnitTests")]

namespace PaymentGateway.Core.Feature.Payments.Services;

public class PaymentServiceCacheDecorator(IPaymentService paymentService, IMemoryCache memoryCache) : IPaymentService
{
    internal readonly IMemoryCache MemoryCache = memoryCache;
    public string GetKey(Guid id, Guid merchantId) => $"{merchantId}_{id}";
    public async Task<Result<GetPaymentResponse?>> GetPaymentAsync(Guid id, Guid merchantId)
    {
        var cacheKey = GetKey(id, merchantId);
        if (MemoryCache.TryGetValue(cacheKey, out GetPaymentResponse? cachedPayment))
        {
            return Result.Ok(cachedPayment);
        }

        var result = await paymentService.GetPaymentAsync(id, merchantId);
        if (result is { IsSuccess: true, Value: not null })
        {
            MemoryCache.Set(cacheKey, result.Value, TimeSpan.FromMinutes(5));
        }

        return result;
    }

    public Task<Result<PostPaymentResponse?>> ProcessPaymentAsync(MerchantPaymentProcessRequest paymentProcessRequest)
    {
        return paymentService.ProcessPaymentAsync(paymentProcessRequest);
    }

    public Task<PostPaymentResponse?> RejectPaymentAsync(MerchantPaymentProcessRequest processRequest)
    {
        return paymentService.RejectPaymentAsync(processRequest);
    }
}