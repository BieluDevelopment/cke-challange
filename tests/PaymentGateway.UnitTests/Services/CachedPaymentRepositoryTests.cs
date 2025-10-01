using AwesomeAssertions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using NSubstitute;

using PaymentGateway.BankIntegration.Services;
using PaymentGateway.Core.Feature.Payments.Services;
using PaymentGateway.Models.Entities.Requests;
using PaymentGateway.PersistantStorage.Dto;
using PaymentGateway.PersistantStorage.Services;

namespace PaymentGateway.UnitTests.Services;

public class CachedPaymentRepositoryTests : PaymentsServiceUnitTests
{
    protected override (IPaymentService service, IBankClient bankClient, IPaymentsRepository repository)
        PrepareForTest()
    {
        var context = base.PrepareForTest();
        Guid guid = Guid.NewGuid();
        var dbContextFactorySub = PrepareSubDbContextFactory(guid);
        var paymentsRepository = new PaymentsRepository(dbContextFactorySub);
        var service = new PaymentServiceCacheDecorator(new PaymentService(context.bankClient, paymentsRepository),
            new MemoryCache(Options.Create(new MemoryCacheOptions())));
        return (service, context.bankClient, paymentsRepository);
    }

    [Fact]
    public async Task GetPayment_Should_Add_Record_To_Cache()
    {
        // Arrange

        var merchantId = Guid.NewGuid();
        // Arrange - card ending with 3 (declined)
        var request = new MerchantPaymentProcessRequest
        {
            CardNumber = "2222405343248873",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Name = "Test User",
            MerchantId = merchantId
        };

        var (service, _, repository) = PrepareForTest();
        var upsertResult = await service.ProcessPaymentAsync(request);
        upsertResult.Should().NotBeNull();
        var paymentId = upsertResult.Value!.Id;
        // Access the cache via the decorator (add a property or method for testing)
        var decorator = (PaymentServiceCacheDecorator)service;
        var memoryCache = decorator.MemoryCache;
//

        var result = await service.GetPaymentAsync(paymentId, merchantId);
        result.IsSuccess.Should().BeTrue();
        var cached = memoryCache.TryGetValue(decorator.GetKey(paymentId, merchantId), out var attempt2);
        cached.Should().BeTrue();
        attempt2.Should().NotBeNull();
    }
}