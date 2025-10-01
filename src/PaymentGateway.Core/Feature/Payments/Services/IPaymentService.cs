using FluentResults;

using PaymentGateway.Models.Entities.Requests;
using PaymentGateway.Models.Entities.Responses;

namespace PaymentGateway.Core.Feature.Payments.Services;

public interface IPaymentService
{
    Task<Result<GetPaymentResponse?>> GetPaymentAsync(Guid id, Guid merchantId);
    Task<Result<PostPaymentResponse?>> ProcessPaymentAsync(MerchantPaymentProcessRequest paymentProcessRequest);
    Task<PostPaymentResponse?> RejectPaymentAsync(MerchantPaymentProcessRequest processRequest);
}