using System.Text.Json.Serialization;

using PaymentGateway.BankIntegration.Serialization;

namespace PaymentGateway.BankIntegration.Models;

public class PaymentResponse
{
    [JsonPropertyName("authorized")]
    public required bool Authorized { get; set; }
    [JsonPropertyName("authorization_code")]
    [JsonConverter(typeof(GuidConverter))]
    public Guid? AuthorizationCode { get; set; }
}