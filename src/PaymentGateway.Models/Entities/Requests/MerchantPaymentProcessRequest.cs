namespace PaymentGateway.Models.Entities.Requests;
public class MerchantPaymentProcessRequest
{
    public string CardNumber { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
    public string Cvv { get; set; }
    public string Name { get; set; }
    public Guid MerchantId { get; set; }
}