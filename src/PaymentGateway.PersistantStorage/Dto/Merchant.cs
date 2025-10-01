namespace PaymentGateway.PersistantStorage.Dto;

public class Merchant
{   public required Guid Id { get; set; }
    public IEnumerable<Payment>? Payments { get; set; }
    public required string ApiKey { get; set; }
    
}