using PaymentGateway.BankIntegration.Models;
using PaymentGateway.BankIntegration.Services;
using PaymentGateway.Models.Entities.Requests;
using PaymentGateway.Models.Entities.Responses;
using PaymentGateway.Models.Enums;
using PaymentGateway.PersistantStorage.Dto;
using PaymentGateway.PersistantStorage.Services;


using FluentResults;

using NSubstitute;
using PaymentGateway.Core.Feature.Payments.Services;

namespace PaymentGateway.UnitTests.Services;

public class PaymentsServiceUnitTests : BaseTest
{
    protected virtual (IPaymentService service, IBankClient bankClient, IPaymentsRepository repository) PrepareForTest()
    {
        var bankClient = Substitute.For<IBankClient>();
        bankClient.SendPayment(Arg.Any<PaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(args =>
            {
                var request = args[0] as PaymentRequest;
                if (request == null)
                {
                    return Result.Fail("internal exception");
                }
                int[] authorisedEnding = [1, 3, 5, 7, 9];
                if (authorisedEnding.Any(x=>request.CardNumber.EndsWith(x.ToString())))
                {
                    return Result.Ok(new PaymentResponse() { Authorized = true, AuthorizationCode = Guid.NewGuid() });
                }

                if (request.CardNumber.EndsWith("0"))
                {
                    return Result.Fail("internal exception");
                }

                return Result.Ok(new PaymentResponse() { Authorized = false });
            }); 
        Guid guid = Guid.NewGuid();
        var paymentsRepository= new PaymentsRepository(PrepareSubDbContextFactory(guid));
        var service = new PaymentService(bankClient, paymentsRepository);

        return (service, bankClient, paymentsRepository);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldReturnSuccess_WhenBankAuthorizes()
    {
        var context = PrepareForTest();
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
            MerchantId = Guid.NewGuid()
        };

        // Act
        var result = await context.service.ProcessPaymentAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(request.Amount, result.Value.Amount);
        Assert.Equal(request.Currency, result.Value.Currency);
        Assert.Equal("8873", result.Value.CardNumberLastFour);
        Assert.Equal(request.ExpiryMonth, result.Value.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, result.Value.ExpiryYear);

    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldReturnDeclined_WhenBankDeclines()
    {
        var context = PrepareForTest();

        // Arrange - card ending with 2 (authorized)
        
        var request = new MerchantPaymentProcessRequest
        {
            CardNumber = "2222405343248872",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Name = "Test User",
            MerchantId = Guid.NewGuid()
        };

        // Act
        var result = await context.service.ProcessPaymentAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldReturnFailure_WhenBankClientFails()
    {
        var context = PrepareForTest();

       
        // Arrange - card ending with 0 (bank client fails)

        var request = new MerchantPaymentProcessRequest
        {
            CardNumber = "2222405343248870",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Name = "Test User",
            MerchantId = Guid.NewGuid()
        };

        // Override bank client to fail
        context.bankClient.SendPayment(Arg.Any<PaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Bank communication error"));

        // Act
        var result = await context.service.ProcessPaymentAsync(request);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldFormatExpiryDateCorrectly()
    {
        var context = PrepareForTest();

        // Arrange
        var request = new MerchantPaymentProcessRequest
        {
            CardNumber = "2222405343248872",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Name = "Test User",
            MerchantId = Guid.NewGuid()
        };

        // Act
        await context.service.ProcessPaymentAsync(request);

        // Assert
        await context.bankClient.Received(1).SendPayment(
            Arg.Is<PaymentRequest>(pr => pr.ExpiryDate == "4/2025"),
            Arg.Any<CancellationToken>()
        );
    }
    

    [Fact]
    public async Task ProcessPaymentAsync_ShouldGenerateUniqueTransactionId()
    {
        var context = PrepareForTest();

        // Arrange
        var request = new MerchantPaymentProcessRequest
        {
            CardNumber = "2222405343248872",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Name = "Test User",
            MerchantId = Guid.NewGuid()
        };

        // Act
        var result = await context.service.ProcessPaymentAsync(request);

        // Assert
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldPassCorrectDataToBankClient()
    {
        var context = PrepareForTest();

        // Arrange
        var request = new MerchantPaymentProcessRequest
        {
            CardNumber = "2222405343248872",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Name = "Test User",
            MerchantId = Guid.NewGuid()
        };

        // Act
        await context.service.ProcessPaymentAsync(request);

        // Assert
        await context.bankClient.Received(1).SendPayment(
            Arg.Is<PaymentRequest>(pr =>
                pr.CardNumber == request.CardNumber &&
                pr.Amount == request.Amount &&
                pr.Currency == request.Currency &&
                pr.Cvv == request.Cvv &&
                pr.ExpiryDate == "4/2025"
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldExtractLastFourDigitsCorrectly()
    {
        var context = PrepareForTest();

        // Arrange
        var request = new MerchantPaymentProcessRequest
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = 12,
            ExpiryYear = 2026,
            Currency = "USD",
            Amount = 500,
            Cvv = "456",
            Name = "Test User",
            MerchantId = Guid.NewGuid()
        };

        // Act
        var result = await context.service.ProcessPaymentAsync(request);

        // Assert
        Assert.Equal("3456", result.Value.CardNumberLastFour);
    }

    [Fact]
    public async Task RejectPaymentAsync_ShouldSavePaymentWithRejectedStatus()
    {
        var context = PrepareForTest();

        // Arrange
        var request = new MerchantPaymentProcessRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Name = "Test User",
            MerchantId = Guid.NewGuid()
        };

        // Act
        var result = await context.service.RejectPaymentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.Rejected, result.Status);
        Assert.Equal("8877", result.CardNumberLastFour);
        Assert.Equal(request.Amount, result.Amount);
        Assert.Equal(request.Currency, result.Currency);

    }


    [Fact]
    public async Task RejectPaymentAsync_ShouldGenerateUniqueTransactionId()
    {
        var context = PrepareForTest();

        // Arrange
        var request = new MerchantPaymentProcessRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Name = "Test User",
            MerchantId = Guid.NewGuid()
        };

        // Act
        var result = await context.service.RejectPaymentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    // GetPaymentAsync Tests
    
    [Fact]
    public async Task GetPaymentAsync_ShouldReturnPayment_WhenExists()
    {
        var context = PrepareForTest();

        // Arrange
        var merchantId = Guid.NewGuid();
        // Arrange
        var request = new MerchantPaymentProcessRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Name = "Test User",
            MerchantId =  merchantId
        };



        // Act
        var paymentId = (await context.service.ProcessPaymentAsync(request)).Value!.Id;

        var result = await context.service.GetPaymentAsync(paymentId, merchantId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(paymentId, result.Value.Id);
        Assert.Equal(PaymentStatus.Authorized, result.Value.Status);
        Assert.Equal("8877", result.Value.CardNumberLastFour);
        Assert.Equal(4, result.Value.ExpiryMonth);
        Assert.Equal(2025, result.Value.ExpiryYear);
        Assert.Equal("GBP", result.Value.Currency);
        Assert.Equal(100, result.Value.Amount);

    }

    [Fact]
    public async Task GetPaymentAsync_ShouldReturnNull_WhenPaymentDoesNotExist()
    {
        var context = PrepareForTest();

        // Arrange
        var paymentId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();


        // Act
        var result = await context.service.GetPaymentAsync(paymentId, merchantId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

   


    [Fact]
    public async Task GetPaymentAsync_ShouldReturnRejectedPayment_WhenPaymentWasRejected()
    {
        var context = PrepareForTest();

        // Arrange
        var merchantId = Guid.NewGuid();
        var request = new MerchantPaymentProcessRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123",
            Name = "Test User",
            MerchantId =  merchantId
        };



        // Act
        var paymentId = (await context.service.RejectPaymentAsync(request))!.Id;


        // Act
        var result = await context.service.GetPaymentAsync(paymentId, merchantId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(PaymentStatus.Rejected, result.Value.Status);
    }

    [Fact]
    public async Task GetPaymentAsync_ShouldNotCallBankClient()
    {
        var context = PrepareForTest();

        // Arrange
        var paymentId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var expectedPayment = new GetPaymentResponse
        {
            Id = paymentId,
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = "8877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100
        };

        // Act
        await context.service.GetPaymentAsync(paymentId, merchantId);

        // Assert
        await context.bankClient.DidNotReceive().SendPayment(
            Arg.Any<PaymentRequest>(),
            Arg.Any<CancellationToken>()
        );
    }

    
}