using System.Text.Json.Serialization;

namespace PaymentGateway.Models.Enums;
[JsonConverter(typeof(JsonStringEnumConverter<PaymentStatus>))]
public enum PaymentStatus
{
    Authorized = 0,
    Declined = 1,
    Rejected = 2
}