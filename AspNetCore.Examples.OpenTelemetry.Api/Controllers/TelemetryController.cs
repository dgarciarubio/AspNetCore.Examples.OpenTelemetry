using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.Api.Controllers;

[ApiController]
[Route("telemetry")]
public class TelemetryController : ControllerBase
{
    private static Meter _meter = new Meter("sample_meter", version: "1.0");
    private static Counter<int> _counter = _meter.CreateCounter<int>(name: "sample_counter", unit: "Units", description: "Sample Counter description");
    private static Histogram<int> _histogram = _meter.CreateHistogram<int>(name: "sample_histogram", unit: "Units", description: "Sample Histogram description");

    private readonly ILogger<TelemetryController> _logger;

    public TelemetryController(ILogger<TelemetryController> logger)
    {
        _logger = logger;
    }

    [HttpGet("traces")]
    public async Task<IActionResult> Traces(int durationMs = 0, string? message = null)
    {
        using var source = new ActivitySource(name: "sample_source", version: "1.0");
        using var trace = source.StartActivity(name: "sample_activity", kind: ActivityKind.Internal, tags: new Dictionary<string, object?>()
        {
            { "Message", message }
        });

        await Task.Delay(durationMs);

        return Ok();
    }

    [HttpGet("metrics")]
    public IActionResult Metrics(int value = 0, string? message = null)
    {
        var tags = new Dictionary<string, object?>()
        {
            { "Message", message }
        }.ToArray();

        _counter.Add(value, tags);
        _histogram.Record(value, tags);

        return Ok();
    }

    [HttpGet("logs")]
    public IActionResult Logs(LogLevel level = LogLevel.Information, string? attributeValue = null)
    {
        _logger.Log(level, "Sample log message with attribute value: \"{attribute}\"", attributeValue);

        return Ok();
    }
}
