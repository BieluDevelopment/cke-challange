using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

using NSubstitute;

using PaymentGateway.Models.Enums;
using PaymentGateway.PersistantStorage;
using PaymentGateway.PersistantStorage.Dto;
using PaymentGateway.PersistantStorage.Services;

namespace PaymentGateway.UnitTests;

public class PaymentsRepositoryUnitTests : BaseTest
{
    private (IPaymentsRepository repository, Guid MerchantId) PrepareForTest()
    {
        Guid guid = Guid.NewGuid();

        var dbContextFactorySub = PrepareSubDbContextFactory(guid);
        Guid testMerchantId = Guid.NewGuid();

        return (new PaymentsRepository(dbContextFactorySub), testMerchantId);
    }

    [Fact]
    public async Task UpsertPaymentAsync_ShouldAddNewPayment()
    {
        var context = PrepareForTest();
        // Arrange
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            MerchantId = context.MerchantId,
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Status = PaymentStatus.Authorized,
            Name = "test user",
        };

        // Act
        await context.repository.UpsertPaymentAsync(payment);

        // Assert
        var result = await context.repository.GetPaymentAsync(payment.Id);
        Assert.NotNull(result);
        Assert.Equal(payment.Id, result.Id);
        Assert.Equal(PaymentStatus.Authorized, result.Status);
        Assert.Equal("GBP", result.Currency);
        Assert.Equal(100, result.Amount);
    }

    [Fact]
    public async Task UpsertPaymentAsync_ShouldUpdateExistingPayment()
    {
        var context = PrepareForTest();
        // Arrange
        var paymentId = Guid.NewGuid();
        var originalPayment = new Payment
        {
            Id = paymentId,
            MerchantId = context.MerchantId,
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Status = PaymentStatus.Declined,
            Name = "test user",
        };

        await context.repository.UpsertPaymentAsync(originalPayment);

        var updatedPayment = new Payment
        {
            Id = paymentId,
            MerchantId = context.MerchantId,
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Status = PaymentStatus.Authorized,
            Name = "test user",
        };

        // Act
        await context.repository.UpsertPaymentAsync(updatedPayment);

        // Assert
        var result = await context.repository.GetPaymentAsync(paymentId);
        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.Authorized, result.Status);
    }

    [Fact]
    public async Task GetPaymentAsync_ShouldReturnPayment_WhenExists()
    {
        var context = PrepareForTest();

        // Arrange
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            MerchantId = context.MerchantId,
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Status = PaymentStatus.Authorized,
            Name = "test user",
        };

        await context.repository.UpsertPaymentAsync(payment);

        // Act
        var result = await context.repository.GetPaymentAsync(payment.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(payment.Id, result.Id);
        Assert.Equal("8877", result.CardNumberLastFour);
        Assert.Equal(4, result.ExpiryMonth);
        Assert.Equal(2025, result.ExpiryYear);
    }

    [Fact]
    public async Task GetPaymentAsync_ShouldReturnNull_WhenNotExists()
    {
        var context = PrepareForTest();

        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await context.repository.GetPaymentAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPayments_ShouldReturnPaymentsForMerchant()
    {
        var context = PrepareForTest();

        // Arrange
        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            MerchantId = context.MerchantId,
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Status = PaymentStatus.Authorized,
            Name = "test user",
        };

        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            MerchantId = context.MerchantId,
            CardNumber = "4444333322221111",
            ExpiryMonth = 12,
            ExpiryYear = 2026,
            Currency = "USD",
            Amount = 200,
            Cvv = "456",
            Status = PaymentStatus.Declined,
            Name = "test user",
        };

        await context.repository.UpsertPaymentAsync(payment1);
        await context.repository.UpsertPaymentAsync(payment2);

        // Act
        var results = await context.repository.GetPayments(context.MerchantId, pageSize: 10, page: 0);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count());
    }

    [Fact]
    public async Task GetPayments_ShouldRespectPagination()
    {
        var context = PrepareForTest();

        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                MerchantId = context.MerchantId,
                CardNumber = "2222405343248877",
                ExpiryMonth = 4,
                ExpiryYear = 2025,
                Currency = "GBP",
                Amount = 100 + i,
                Cvv = "123",
                Status = PaymentStatus.Authorized,
                Name = "test user",
            };
            await context.repository.UpsertPaymentAsync(payment);
        }

        // Act
        var firstPage = await context.repository.GetPayments(context.MerchantId, pageSize: 2, page: 0);
        var secondPage = await context.repository.GetPayments(context.MerchantId, pageSize: 2, page: 1);

        // Assert
        Assert.Equal(2, firstPage.Count());
        Assert.Equal(2, secondPage.Count());
    }

    [Fact]
    public async Task GetPayments_ShouldReturnEmpty_WhenNoPaymentsExist()
    {
        var context = PrepareForTest();

        // Arrange
        var emptyMerchantId = Guid.NewGuid();

        // Act
        var results = await context.repository.GetPayments(emptyMerchantId, pageSize: 10, page: 0);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task DeletePaymentAsync_ShouldRemovePayment()
    {
        var context = PrepareForTest();

        // Arrange
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            MerchantId = context.MerchantId,
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Status = PaymentStatus.Authorized,
            Name = "test user",
        };

        await context.repository.UpsertPaymentAsync(payment);

        // Act
        await context.repository.DeletePaymentAsync(payment.Id);

        // Assert
        var result = await context.repository.GetPaymentAsync(payment.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeletePaymentAsync_ShouldNotThrow_WhenPaymentDoesNotExist()
    {
        var context = PrepareForTest();

        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await context.repository.DeletePaymentAsync(nonExistentId));

        Assert.Null(exception);
    }

    [Fact]
    public async Task GetPayments_ShouldOnlyReturnPaymentsForSpecificMerchant()
    {
        var context = PrepareForTest();

        // Arrange
        var merchant1Id = Guid.NewGuid();
        var merchant2Id = Guid.NewGuid();

        var payment1 = new Payment
        {
            Id = Guid.NewGuid(),
            MerchantId = merchant1Id,
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Status = PaymentStatus.Authorized,
            Name = "test user",
        };

        var payment2 = new Payment
        {
            Id = Guid.NewGuid(),
            MerchantId = merchant2Id,
            CardNumber = "4444333322221111",
            ExpiryMonth = 12,
            ExpiryYear = 2026,
            Currency = "USD",
            Amount = 200,
            Cvv = "456",
            Status = PaymentStatus.Declined,
            Name = "test user",
        };

        await context.repository.UpsertPaymentAsync(payment1);
        await context.repository.UpsertPaymentAsync(payment2);

        // Act
        var merchant1Results = await context.repository.GetPayments(merchant1Id, pageSize: 10, page: 0);
        var merchant2Results = await context.repository.GetPayments(merchant2Id, pageSize: 10, page: 0);

        // Assert
        Assert.Single(merchant1Results);
        Assert.Single(merchant2Results);
        Assert.Equal(payment1.Id, merchant1Results.First().Id);
        Assert.Equal(payment2.Id, merchant2Results.First().Id);
    }

    public void Dispose()
    {
    }
}