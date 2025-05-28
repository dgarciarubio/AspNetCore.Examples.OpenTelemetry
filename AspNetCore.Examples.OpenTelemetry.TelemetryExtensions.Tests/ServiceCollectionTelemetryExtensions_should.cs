using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata.Ecma335;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryExtensions.Tests;

public class ServiceCollectionTelemetryExtensions_should
{
    private readonly IServiceCollection _services;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMeterFactory _meterFactory;

    public ServiceCollectionTelemetryExtensions_should()
    {
        _services = new ServiceCollection();
        _logger = Substitute.For<ILogger>();
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(_logger);
        _meterFactory = Substitute.For<IMeterFactory>();
        _meterFactory.Create(Arg.Any<MeterOptions>()).Returns((callInfo) =>
        {
            var options = callInfo.Arg<MeterOptions>();
            return new Meter(options);
        });
        _services.AddSingleton(_loggerFactory);
        _services.AddSingleton(_meterFactory);
    }

    [Fact]
    public void Fail_if_null_services()
    {
        Action[] actions = [
            () => ServiceCollectionTelemetryExtensions.AddTelemetry(services: null!, "Name"),
            () => ServiceCollectionTelemetryExtensions.AddTelemetry(services: null!, new TelemetryOptions("Name")),
            () => ServiceCollectionTelemetryExtensions.AddTelemetry<TelemetryName>(services: null!),
            () => ServiceCollectionTelemetryExtensions.AddTelemetry<TelemetryName>(services: null!, new()),
        ];

        Assert.All(actions, action =>
        {
            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("services", exception.ParamName);
        });
    }

    [Fact]
    public void Fail_if_null_name()
    {
        Action action = () => ServiceCollectionTelemetryExtensions.AddTelemetry(_services, name: null!);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Fail_if_null_options()
    {
        Action action = () => ServiceCollectionTelemetryExtensions.AddTelemetry(_services, options: null!);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal("options", exception.ParamName);
    }

    [Fact]
    public void Register_telemetry_services_by_name()
    {
        var name = "Name";

        _services.AddTelemetry(name);

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetKeyedService<ITelemetry>(name);
        Assert.NotNull(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.Equal(name, telemetry.ActivitySource.Name);
        Assert.NotNull(telemetry.Meter);
        Assert.Equal(name, telemetry.Meter.Name);
    }

    [Theory]
    [ClassData(typeof(TelemetryOptionsData))]
    public void Register_telemetry_services_by_options(TelemetryOptions options)
    {
        _services.AddTelemetry(options);

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetKeyedService<ITelemetry>(options.Name);
        Assert.NotNull(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }

    [Fact]
    public void Register_telemetry_services_by_generic_type()
    {
        _services.AddTelemetry<TelemetryName>();

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetKeyedService<ITelemetry>(TelemetryOptions<TelemetryName>.Name);
        var genericTelemetry = serviceProvider.GetService<ITelemetry<TelemetryName>>();
        Assert.NotNull(telemetry);
        Assert.NotNull(genericTelemetry);
        Assert.Same(telemetry, genericTelemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.Equal(TelemetryOptions<TelemetryName>.Name, telemetry.ActivitySource.Name);
        Assert.NotNull(telemetry.Meter);
        Assert.Equal(TelemetryOptions<TelemetryName>.Name, telemetry.Meter.Name);
    }

    [Theory]
    [ClassData(typeof(GenericTelemetryOptionsData))]
    public void Register_telemetry_services_by_generic_type_options(TelemetryOptions<TelemetryName>? options)
    {
        _services.AddTelemetry(options);

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetKeyedService<ITelemetry>(TelemetryOptions<TelemetryName>.Name);
        var genericTelemetry = serviceProvider.GetService<ITelemetry<TelemetryName>>();
        Assert.NotNull(telemetry);
        Assert.NotNull(genericTelemetry);
        Assert.Same(telemetry, genericTelemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }
}