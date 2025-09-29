using FluentResults;

using PaymentGateway.BankIntegration.Models;

namespace PaymentGateway.BankIntegration.Services;

public interface IBankClient
{
    public Task<Result<PaymentResponse>> SendPayment(PaymentRequest paymentRequest, CancellationToken cancellationToken = default);
}