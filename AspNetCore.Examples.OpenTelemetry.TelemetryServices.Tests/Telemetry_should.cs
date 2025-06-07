using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Testing.Platform.Extensions.Messages;
using NSubstitute;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using static AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests.Generic_telemetry_should;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests;

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
        var action = () => new Telemetry(loggerFactory: null!, _meterFactory, new TelemetryOptions { Name = "Name" });

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal("loggerFactory", exception.ParamName);
    }

    [Fact]
    public void Fail_if_null_meterFactory()
    {
        var action = () => new Telemetry(_loggerFactory, meterFactory: null!, new TelemetryOptions { Name = "Name" });

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal("meterFactory", exception.ParamName);
    }

    [Fact]
    public void Fail_if_null_options()
    {
        var action = () => new Telemetry(_loggerFactory, _meterFactory, options: null!);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal("options", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(OptionsData))]
    public void Create_a_logger(TelemetryOptions options)
    {
        var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Logger);
    }

    [Theory]
    [MemberData(nameof(OptionsData))]
    public void Create_an_activity_source(TelemetryOptions options)
    {
        var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
    }

    [Theory]
    [MemberData(nameof(OptionsData))]
    public void Create_a_meter(TelemetryOptions options)
    {
        var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }

    public static readonly TelemetryOptionsData OptionsData = [];
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
        var action = () => new Telemetry<TelemetryName>(loggerFactory: null!, _meterFactory);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal("loggerFactory", exception.ParamName);
    }

    [Fact]
    public void Fail_if_null_meterFactory()
    {
        var action = () => new Telemetry<TelemetryName>(_loggerFactory, meterFactory: null!);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal("meterFactory", exception.ParamName);
    }

    [Fact]
    public void Fail_if_invalid_options()
    {
        var action = () => new Telemetry<TelemetryName>(_loggerFactory, _meterFactory, options: new() { Name = "InvalidName" });

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("options", exception.ParamName);
    }

    [Fact]
    public void Accept_null_options()
    {
        var telemetry = new Telemetry<TelemetryName>(_loggerFactory, _meterFactory, options: null);

        Assert.Equal(Telemetry<TelemetryName>.Name, telemetry.ActivitySource.Name);
        Assert.Equal(Telemetry<TelemetryName>.Name, telemetry.Meter.Name);
    }

    [Fact]
    public void Have_the_expected_name()
    {
        var name = Telemetry<TelemetryName>.Name;

        Assert.Equal($"{typeof(Generic_telemetry_should).Namespace}.{nameof(Generic_telemetry_should)}.{nameof(TelemetryName)}", name);
    }

    [Theory]
    [MemberData(nameof(OptionsData))]
    public void Create_a_logger(TelemetryOptions options)
    {
        var telemetry = new Telemetry<TelemetryName>(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Logger);
    }

    [Theory]
    [MemberData(nameof(OptionsData))]
    public void Create_an_activity_source(TelemetryOptions options)
    {
        var telemetry = new Telemetry<TelemetryName>(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
    }

    [Theory]
    [MemberData(nameof(OptionsData))]
    public void Create_a_meter(TelemetryOptions options)
    {
        var telemetry = new Telemetry<TelemetryName>(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }

    public static readonly NamedTelemetryOptionsData OptionsData = new(Telemetry<TelemetryName>.Name);

    private class TelemetryName { }
}
