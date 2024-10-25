using FluentAssertions;
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
        var action = () => new Telemetry(loggerFactory: null!, _meterFactory, new TelemetryOptions("Name"));

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("loggerFactory");
    }

    [Fact]
    public void Fail_if_null_meterFactory()
    {
        var action = () => new Telemetry(_loggerFactory, meterFactory: null!, new TelemetryOptions("Name"));

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("meterFactory");
    }

    [Fact]
    public void Fail_if_null_name()
    {
        var action = () => new Telemetry(_loggerFactory, _meterFactory, options: null!);

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("options");
    }

    [Theory]
    [ClassData(typeof(TelemetryParamsData))]
    public void Create_a_logger(TelemetryOptions options)
    {
        var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        telemetry.Logger.Should().NotBeNull();
    }

    [Theory]
    [ClassData(typeof(TelemetryParamsData))]
    public void Create_an_activity_source(TelemetryOptions options)
    {
        var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        telemetry.Meter.Should().NotBeNull();
        telemetry.Meter.Name.Should().Be(options.Name);
        telemetry.Meter.Version.Should().Be(options?.Version);
        telemetry.Meter.Tags.Should().BeEquivalentTo(options?.Tags);
        telemetry.Meter.Scope.Should().BeEquivalentTo(options?.Scope);
    }

    [Theory]
    [ClassData(typeof(TelemetryParamsData))]
    public void Create_a_meter(TelemetryOptions options)
    {
        var telemetry = new Telemetry(_loggerFactory, _meterFactory, options);

        telemetry.Meter.Should().NotBeNull();
        telemetry.Meter.Name.Should().Be(options.Name);
        telemetry.Meter.Version.Should().Be(options?.Version);
        telemetry.Meter.Tags.Should().BeEquivalentTo(options?.Tags);
        telemetry.Meter.Scope.Should().BeEquivalentTo(options?.Scope);
    }

    private class TelemetryParamsData : IEnumerable<object?[]>
    {
        public IEnumerator<object?[]> GetEnumerator()
        {
            yield return [new TelemetryOptions("Name")];
            yield return [new TelemetryOptions ("Name")
            {
                Version = "V1.0",
                Tags = new Dictionary<string, object?> { { "TagName", "TagValue" } },
                Scope = "Scope",
            }];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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
        var action = () => new Telemetry<Category>(loggerFactory: null!, _meterFactory);

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("loggerFactory");
    }

    [Fact]
    public void Fail_if_null_meterFactory()
    {
        var action = () => new Telemetry<Category>(_loggerFactory, meterFactory: null!);

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("meterFactory");
    }

    [Theory]
    [ClassData(typeof(TelemetryParamsData))]
    public void Create_a_logger(TelemetryOptions<Category>? options)
    {
        var telemetry = new Telemetry<Category>(_loggerFactory, _meterFactory, options);

        telemetry.Logger.Should().NotBeNull();
        telemetry.Logger.Should().Be(((Telemetry)telemetry).Logger);
    }

    [Theory]
    [ClassData(typeof(TelemetryParamsData))]
    public void Create_an_activity_source(TelemetryOptions<Category>? options)
    {
        var telemetry = new Telemetry<Category>(_loggerFactory, _meterFactory, options);

        telemetry.Meter.Should().NotBeNull();
        telemetry.Meter.Name.Should().Be(TelemetryOptions<Category>.Name);
        telemetry.Meter.Version.Should().Be(options?.Version);
        telemetry.Meter.Tags.Should().BeEquivalentTo(options?.Tags);
        telemetry.Meter.Scope.Should().BeEquivalentTo(options?.Scope);
    }

    [Theory]
    [ClassData(typeof(TelemetryParamsData))]
    public void Create_a_meter(TelemetryOptions<Category>? options)
    {
        var telemetry = new Telemetry<Category>(_loggerFactory, _meterFactory, options);

        telemetry.Meter.Should().NotBeNull();
        telemetry.Meter.Name.Should().Be(TelemetryOptions<Category>.Name);
        telemetry.Meter.Version.Should().Be(options?.Version);
        telemetry.Meter.Tags.Should().BeEquivalentTo(options?.Tags);
        telemetry.Meter.Scope.Should().BeEquivalentTo(options?.Scope);
    }

    private class TelemetryParamsData : IEnumerable<object?[]>
    {
        public IEnumerator<object?[]> GetEnumerator()
        {
            yield return [null];
            yield return [new TelemetryOptions<Category>
            {
                Version = "V1.0",
                Tags = new Dictionary<string, object?> { { "TagName", "TagValue" } },
                Scope = "Scope",
            }];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

public class Category
{
}