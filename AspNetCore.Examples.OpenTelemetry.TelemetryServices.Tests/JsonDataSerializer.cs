using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Xunit.Sdk;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests;

internal class JsonDataSerializer : IXunitSerializer
{
    public bool IsSerializable(Type type, object? value, [NotNullWhen(false)] out string? failureReason)
    {
        failureReason = null;
        if (value is null)
        {
            failureReason = "Value cannot be null";
            return false;
        }
        return true;
    }

    public string Serialize(object value) => JsonSerializer.Serialize(value);

    public object Deserialize(Type type, string serializedValue) => JsonSerializer.Deserialize(serializedValue, type)
        ?? throw new ArgumentException("Serialized value results in null object", nameof(serializedValue));
}