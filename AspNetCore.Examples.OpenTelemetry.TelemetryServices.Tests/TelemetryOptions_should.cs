using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests;

public class TelemetryOptions_should
{
    [Theory]
    [ClassData(typeof(TelemetryOptionsData))]
    public void Be_initialized_properly(TelemetryOptions optionsData)
    {
        var options = new TelemetryOptions
        {
            Name = optionsData.Name,
            Version = optionsData.Version,
            Tags = optionsData.Tags,
        };

        Assert.Equal(optionsData.Name, options.Name);
        Assert.Equal(optionsData.Version, options.Version);
        Assert.Equal(optionsData.Tags, options.Tags);
    }

    [Fact]
    public void Not_be_initialized_with_a_null_name()
    {
        var action = () => new TelemetryOptions { Name = null! };

        var exception = Assert.Throws<ArgumentNullException>(() => action());
        Assert.Equal("value", exception.ParamName);
    }
}

public class TelemetryOptionsTService_should
{
    [Theory]
    [ClassData(typeof(TelemetryOptionsData))]
    public void Be_initialized_properly(TelemetryOptions optionsData)
    {
        var options = new TelemetryOptions<TelemetryService>
        {
            Name = optionsData.Name,
            Version = optionsData.Version,
            Tags = optionsData.Tags,
        };

        Assert.Equal(optionsData.Name, options.Name);
        Assert.Equal(optionsData.Version, options.Version);
        Assert.Equal(optionsData.Tags, options.Tags);
    }

    [Fact]
    public void Not_be_initialized_with_a_null_name()
    {
        var action = () => new TelemetryOptions<TelemetryService> { Name = null! };

        var exception = Assert.Throws<ArgumentNullException>(() => action());
        Assert.Equal("value", exception.ParamName);
    }

    private class TelemetryService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<TelemetryService> options) 
        : Telemetry(loggerFactory, meterFactory, options)
    {
    }
}