using AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests.Extensions;
using AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests.TestDoubles;
using AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests.TheoryData;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Diagnostics;
using System.Diagnostics.Metrics;

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
        using var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Logger);
    }

    [Fact]
    public void Create_logs()
    {
        var options = new TelemetryOptions { Name = "Name" };
        using var listener = new LoggingListener();

        using var telemetry = new Telemetry(listener, _meterFactory, options);
        telemetry.Logger.LogDebug("Log");

        var logged = listener.Logs.FirstOrDefault();
        Assert.NotNull(logged);
        Assert.Equal("Log", logged.Message);
        Assert.Equal(options.Name, logged.CategoryName);
    }

    [Theory]
    [MemberData(nameof(OptionsData))]
    public void Create_an_activity_source(TelemetryOptions options)
    {
        using var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options.ActivitySource, telemetry.ActivitySource);
    }

    [Fact]
    public void Create_activities()
    {
        var options = new TelemetryOptions { Name = "Name" };
        Activity? startedActivity = null;
        using var listener = new ActivityListener()
        {
            ShouldListenTo = source => source.Name == options.Name,
            Sample = (ref _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => startedActivity = activity,
        };
        ActivitySource.AddActivityListener(listener);

        using var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);
        using var activity = telemetry.ActivitySource.StartActivity();

        Assert.NotNull(startedActivity);
        Assert.Same(activity, startedActivity);
    }

    [Theory]
    [MemberData(nameof(OptionsData))]
    public void Create_a_meter(TelemetryOptions options)
    {
        using var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options.Meter, telemetry.Meter);
    }

    [Fact]
    public void Create_metrics()
    {
        var options = new TelemetryOptions { Name = "Name" };
        int? recordedMeasurement = null;
        using var listener = new MeterListener()
        {
            InstrumentPublished = (instrument, listener) => listener.EnableMeasurementEvents(instrument),
        };
        listener.SetMeasurementEventCallback<int>((_, measurement, _, _) => recordedMeasurement = measurement);
        listener.Start();

        using var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);
        telemetry.Meter.CreateCounter<int>("Counter").Add(1);

        Assert.NotNull(recordedMeasurement);
    }

    [Fact]
    public void Dispose_related_telemetry_elements()
    {
        var options = new TelemetryOptions { Name = "Name" };
        int? recordedMeasurement = null;
        using var meterListener = new MeterListener()
        {
            InstrumentPublished = (instrument, listener) => listener.EnableMeasurementEvents(instrument),
        };
        meterListener.SetMeasurementEventCallback<int>((_, measurement, _, _) => recordedMeasurement = measurement);
        meterListener.Start();
        Activity? startedActivity = null;
        using var listener = new ActivityListener()
        {
            ShouldListenTo = source => source.Name == options.Name,
            Sample = (ref _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => startedActivity = activity,
        };
        ActivitySource.AddActivityListener(listener);

        var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);
        telemetry.Dispose();
        telemetry.Meter.CreateCounter<int>("Counter").Add(1);
        using var activity = telemetry.ActivitySource.StartActivity();

        Assert.Null(recordedMeasurement);
        Assert.Null(startedActivity);
        Assert.Null(activity);
    }

    public static readonly TelemetryOptionsData OptionsData = [];
}

public class TelemetryTTelemetryName_should
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMeterFactory _meterFactory;

    public TelemetryTTelemetryName_should()
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

        Assert.Equal($"{typeof(TelemetryTTelemetryName_should).Namespace}.{nameof(TelemetryTTelemetryName_should)}.{nameof(TelemetryName)}", name);
    }

    [Theory]
    [MemberData(nameof(OptionsData))]
    public void Create_a_logger(TelemetryOptions options)
    {
        var telemetry = new Telemetry<TelemetryName>(_loggerFactory, _meterFactory, options);

        Assert.NotNull(telemetry.Logger);
    }

    [Fact]
    public void Create_logs()
    {
        using var listener = new LoggingListener();

        using var telemetry = new Telemetry<TelemetryName>(listener, _meterFactory);
        telemetry.Logger.LogDebug("Log");

        var logged = listener.Logs.FirstOrDefault();
        Assert.NotNull(logged);
        Assert.Equal("Log", logged.Message);
        Assert.Equal(Telemetry<TelemetryName>.Name, logged.CategoryName);
    }

    public static readonly TelemetryOptionsData OptionsData = new(Telemetry<TelemetryName>.Name);

    private class TelemetryName { }
}


