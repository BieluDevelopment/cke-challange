using FluentResults;

using PaymentGateway.BankIntegration.Models;
using PaymentGateway.BankIntegration.Services;
using PaymentGateway.Models.Entities.Requests;
using PaymentGateway.Models.Entities.Responses;
using PaymentGateway.Models.Enums;
using PaymentGateway.PersistantStorage.Dto;
using PaymentGateway.PersistantStorage.Services;

namespace PaymentGateway.Core.Feature.Payments.Services;

public class PaymentService(IBankClient bankClient, IPaymentsRepository paymentsRepository) :IPaymentService
{
    public async Task<Result<GetPaymentResponse?>> GetPaymentAsync(Guid id, Guid merchantId)
    {
        var result=  await paymentsRepository.GetPaymentAsync(id);
        return result;
    }

    public async Task<Result<PostPaymentResponse?>> ProcessPaymentAsync(MerchantPaymentProcessRequest paymentProcessRequest)
    {
        
        var result=  await bankClient.SendPayment(new PaymentRequest
        {
            Amount = paymentProcessRequest.Amount ?? 0,
            Currency = paymentProcessRequest.Currency,
            CardNumber = paymentProcessRequest.CardNumber,
            Cvv = paymentProcessRequest.Cvv,
            ExpiryDate = $"{paymentProcessRequest.ExpiryMonth}/{paymentProcessRequest.ExpiryYear}",

        });
        if (result.IsFailed)
        {
            return result.ToResult<PostPaymentResponse?>();
        }

        var transactionId = Guid.NewGuid();
        var status = result.Value.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined;
      await paymentsRepository.UpsertPaymentAsync(new Payment
        {
            Id = transactionId,
            Amount = paymentProcessRequest.Amount ??0,
            Currency = paymentProcessRequest.Currency,
            CardNumber = paymentProcessRequest.CardNumber,
            Cvv = paymentProcessRequest.Cvv,
            ExpiryMonth = paymentProcessRequest.ExpiryMonth ?? 1,
            ExpiryYear = paymentProcessRequest.ExpiryYear,
            Name = paymentProcessRequest.Name,
            MerchantId = paymentProcessRequest.MerchantId,
            AuthorizationCode = result.Value.AuthorizationCode,
            Status =status 
        });
        return Result.Ok(ToDomainModel(paymentProcessRequest,transactionId,status));
    }

    public async Task<PostPaymentResponse?> RejectPaymentAsync(MerchantPaymentProcessRequest paymentProcessRequest)
    {
        var transactionId = Guid.NewGuid();
        await paymentsRepository.UpsertPaymentAsync(new Payment
        {
            Id =transactionId,
            Amount = paymentProcessRequest.Amount ?? 0,
            Currency = paymentProcessRequest.Currency,
            CardNumber = paymentProcessRequest.CardNumber,
            Cvv = paymentProcessRequest.Cvv,
            ExpiryMonth = paymentProcessRequest.ExpiryMonth ??1,
            ExpiryYear = paymentProcessRequest.ExpiryYear,
            Name = paymentProcessRequest.Name,
            MerchantId = paymentProcessRequest.MerchantId,
            Status = PaymentStatus.Rejected
        });
        return ToDomainModel(paymentProcessRequest,transactionId,PaymentStatus.Rejected);
    }

    private PostPaymentResponse? ToDomainModel(MerchantPaymentProcessRequest paymentProcessRequest, Guid transactionId, PaymentStatus paymentStatus)
    {
        return new PostPaymentResponse()
        {
            Id = transactionId,
            Amount = paymentProcessRequest.Amount ?? 0,
            Currency = paymentProcessRequest.Currency,
            CardNumberLastFour =
                paymentProcessRequest.CardNumber.Substring(paymentProcessRequest.CardNumber.Length - 4, 4),
            ExpiryMonth = paymentProcessRequest.ExpiryMonth ?? 1,
            ExpiryYear = paymentProcessRequest.ExpiryYear,
            Status = paymentStatus
        };
    }
}