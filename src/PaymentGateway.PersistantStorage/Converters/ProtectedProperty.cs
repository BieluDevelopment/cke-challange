namespace PaymentGateway.PersistantStorage.Converters;

// public class ProtectedProperty<T> : ValueConverter<T?,string>
// {
//     public ProtectedProperty() : base(
//         d => JsonSerializer.Serialize<T?>(d).Encrypt(),
//         d => string.IsNullOrWhiteSpace( d.Decrypt() ?? "") ? default :JsonSerializer.Deserialize<T>(d.Decrypt() ?? ""))
//     {
//
//     }
// }