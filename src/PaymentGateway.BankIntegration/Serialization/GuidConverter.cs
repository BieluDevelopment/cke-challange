using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentGateway.BankIntegration.Serialization;

internal sealed class GuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return Guid.Empty;
            case JsonTokenType.String:
                string? str = reader.GetString();
                if (string.IsNullOrEmpty(str))
                {
                    return Guid.Empty;
                }

                return reader.GetGuid();
            default:
                throw new ArgumentException("Invalid token type");
        }
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }

}