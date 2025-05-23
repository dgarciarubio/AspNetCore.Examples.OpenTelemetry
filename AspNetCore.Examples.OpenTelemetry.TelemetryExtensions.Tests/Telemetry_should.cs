using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Collections;
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
    public void Fail_if_null_name()
    {
        Telemetry action() => new(_loggerFactory, _meterFactory, options: null!);

        var exception = Assert.Throws<ArgumentNullException>((Func<Telemetry>)action);
        Assert.Equal("options", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(TelemetryOptionsData))]
    public void Create_a_logger(TelemetryOptions options)
    {
        var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Logger);
    }

    [Theory]
    [MemberData(nameof(TelemetryOptionsData))]
    public void Create_an_activity_source(TelemetryOptions options)
    {
        var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.ActivitySource);
        Assert.Equal(options.Name, telemetry.ActivitySource.Name);
        Assert.Equal(options.Version, telemetry.ActivitySource.Version);
        Assert.Equal(options.Tags, telemetry.ActivitySource.Tags);
    }

    [Theory]
    [MemberData(nameof(TelemetryOptionsData))]
    public void Create_a_meter(TelemetryOptions options)
    {
        var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Meter);
        Assert.Equal(options.Name, telemetry.Meter.Name);
        Assert.Equal(options.Version, telemetry.Meter.Version);
        Assert.Equal(options.Tags, telemetry.Meter.Tags);
        Assert.Equal(options.Scope, telemetry.Meter.Scope);
    }

    public static TheoryData<TelemetryOptions> TelemetryOptionsData => new()
    {
        { new TelemetryOptions("Name") },
        { new TelemetryOptions ("Name")
        {
            Version = "V1.0",
            Tags = new Dictionary<string, object?> { { "TagName", "TagValue" } },
            Scope = "Scope",
        }},
    };
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
        Telemetry<Category> action() => new(loggerFactory: null!, _meterFactory);

        var exception = Assert.Throws<ArgumentNullException>((Func<Telemetry<Category>>)action);
        Assert.Equal("loggerFactory", exception.ParamName);
    }

    [Fact]
    public void Fail_if_null_meterFactory()
    {
        Telemetry<Category> action() => new(_loggerFactory, meterFactory: null!);

        var exception = Assert.Throws<ArgumentNullException>((Func<Telemetry<Category>>)action);
        Assert.Equal("meterFactory", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(TelemetryOptionsData))]
    public void Create_a_logger(TelemetryOptions<Category>? options)
    {
        var telemetry = new Telemetry<Category>(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Logger);
        Assert.Same(((Telemetry)telemetry).Logger, telemetry.Logger);
    }

    [Theory]
    [MemberData(nameof(TelemetryOptionsData))]
    public void Create_an_activity_source(TelemetryOptions<Category>? options)
    {
        var telemetry = new Telemetry<Category>(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.ActivitySource);
        Assert.Equal(TelemetryOptions<Category>.Name, telemetry.ActivitySource.Name);
        Assert.Equal(options?.Version, telemetry.ActivitySource.Version);
        Assert.Equal(options?.Tags, telemetry.ActivitySource.Tags);
    }

    [Theory]
    [MemberData(nameof(TelemetryOptionsData))]
    public void Create_a_meter(TelemetryOptions<Category>? options)
    {
        var telemetry = new Telemetry<Category>(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Meter);
        Assert.Equal(TelemetryOptions<Category>.Name, telemetry.Meter.Name);
        Assert.Equal(options?.Version, telemetry.Meter.Version);
        Assert.Equal(options?.Tags, telemetry.Meter.Tags);
        Assert.Equal(options?.Scope, telemetry.Meter.Scope);
    }

    public static TheoryData<TelemetryOptions<Category>?> TelemetryOptionsData => new()
    {
        { null },
        { new TelemetryOptions<Category>
        {
            Version = "V1.0",
            Tags = new Dictionary<string, object?> { { "TagName", "TagValue" } },
            Scope = "Scope",
        }},
    };
}

public class Category
{
}