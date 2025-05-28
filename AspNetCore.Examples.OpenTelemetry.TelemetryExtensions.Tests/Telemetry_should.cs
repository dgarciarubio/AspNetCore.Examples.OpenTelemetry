using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryExtensions.Tests;

public class Telemetry_should
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMeterFactory _meterFactory;

    public Telemetry_should()
    {
        _logger = Substitute.For<ILogger>();
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(_logger);
        _meterFactory = Substitute.For<IMeterFactory>();
        _meterFactory.Create(Arg.Any<MeterOptions>()).Returns((callInfo) =>
        {
            var options = callInfo.Arg<MeterOptions>();
            return new Meter(options);
        });
    }

    [Fact]
    public void Fail_if_null_loggerFactory()
    {
        Telemetry action() => new(loggerFactory: null!, _meterFactory, new TelemetryOptions("Name"));

        var exception = Assert.Throws<ArgumentNullException>((Func<Telemetry>)action);
        Assert.Equal("loggerFactory", exception.ParamName);
    }

    [Fact]
    public void Fail_if_null_meterFactory()
    {
        Telemetry action() => new(_loggerFactory, meterFactory: null!, new TelemetryOptions("Name"));

        var exception = Assert.Throws<ArgumentNullException>((Func<Telemetry>)action);
        Assert.Equal("meterFactory", exception.ParamName);
    }

    [Fact]
    public void Fail_if_null_options()
    {
        Telemetry action() => new(_loggerFactory, _meterFactory, options: null!);

        var exception = Assert.Throws<ArgumentNullException>((Func<Telemetry>)action);
        Assert.Equal("options", exception.ParamName);
    }

    [Theory]
    [ClassData(typeof(TelemetryOptionsData))]
    public void Create_a_logger(TelemetryOptions options)
    {
        var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Logger);
    }

    [Theory]
    [ClassData(typeof(TelemetryOptionsData))]
    public void Create_an_activity_source(TelemetryOptions options)
    {
        var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
    }

    [Theory]
    [ClassData(typeof(TelemetryOptionsData))]
    public void Create_a_meter(TelemetryOptions options)
    {
        var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }
}

public class Generic_telemetry_should
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMeterFactory _meterFactory;

    public Generic_telemetry_should()
    {
        _logger = Substitute.For<ILogger>();
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(_logger);
        _meterFactory = Substitute.For<IMeterFactory>();
        _meterFactory.Create(Arg.Any<MeterOptions>()).Returns((callInfo) =>
        {
            var options = callInfo.Arg<MeterOptions>();
            return new Meter(options);
        });
    }

    [Fact]
    public void Fail_if_null_loggerFactory()
    {
        Telemetry<TelemetryName> action() => new(loggerFactory: null!, _meterFactory);

        var exception = Assert.Throws<ArgumentNullException>((Func<Telemetry<TelemetryName>>)action);
        Assert.Equal("loggerFactory", exception.ParamName);
    }

    [Fact]
    public void Fail_if_null_meterFactory()
    {
        Telemetry<TelemetryName> action() => new(_loggerFactory, meterFactory: null!);

        var exception = Assert.Throws<ArgumentNullException>((Func<Telemetry<TelemetryName>>)action);
        Assert.Equal("meterFactory", exception.ParamName);
    }

    [Theory]
    [ClassData(typeof(TelemetryOptionsData<TelemetryName>))]
    public void Create_a_logger(TelemetryOptions<TelemetryName>? options)
    {
        var telemetry = new Telemetry<TelemetryName>(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Logger);
        Assert.Same(((Telemetry)telemetry).Logger, telemetry.Logger);
    }

    [Theory]
    [ClassData(typeof(TelemetryOptionsData<TelemetryName>))]
    public void Create_an_activity_source(TelemetryOptions<TelemetryName>? options)
    {
        var telemetry = new Telemetry<TelemetryName>(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
    }

    [Theory]
    [ClassData(typeof(TelemetryOptionsData<TelemetryName>))]
    public void Create_a_meter(TelemetryOptions<TelemetryName>? options)
    {
        var telemetry = new Telemetry<TelemetryName>(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }

    public class TelemetryName { }
}
