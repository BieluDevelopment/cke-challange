using PaymentGateway.Models.Enums;

namespace PaymentGateway.PersistantStorage.Dto;

public class Payment
{
    public required Guid Id { get; set; }
    public required string Name { get;set; }
    public required Guid MerchantId { get;set; }
    public Merchant? Merchant { get; set; }
    public required string CardNumber { get; set; }
    public required  string Cvv { get; set; }
    public int Amount { get; set; }
    public string Currency { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public PaymentStatus Status { get; set; }
    public Guid? AuthorizationCode { get; set; }
}