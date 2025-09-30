using PaymentGateway.Models.Entities.Responses;
using PaymentGateway.PersistantStorage.Dto;

namespace PaymentGateway.PersistantStorage.Services;

public interface IPaymentsRepository  : IDisposable
{
    Task<IEnumerable<GetPaymentResponse?>> GetPayments(Guid merchantId, int pageSize, int page);
    Task<GetPaymentResponse?> GetPaymentAsync(Guid paymentID);
    Task UpsertPaymentAsync(Payment payment);
    Task DeletePaymentAsync(Guid paymentID);
}