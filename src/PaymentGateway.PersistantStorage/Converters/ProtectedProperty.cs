using System.Text.Json;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using PaymentGateway.PersistantStorage.Extensions;

namespace PaymentGateway.PersistantStorage.Converters;

public class ProtectedProperty<T> : ValueConverter<T?,string>
{
    public ProtectedProperty() : base(
        d => JsonSerializer.Serialize(d, JsonSerializerOptions.Web).Encrypt() ?? "",
        d => string.IsNullOrWhiteSpace( d.Decrypt() ?? "") ? default :JsonSerializer.Deserialize<T>(d.Decrypt() ?? "", JsonSerializerOptions.Web))
    {

    }
}
