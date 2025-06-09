using AspNetCore.Examples.OpenTelemetry.TelemetryServices;
using AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(typeof(JsonDataSerializer),
    typeof(TelemetryOptions),
    typeof(IEnumerable<KeyValuePair<string, object?>>),
    typeof(int),
    typeof(string),
    typeof(DateTime),
    typeof(object)
)]

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