using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Xml.Linq;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests;

public class ServiceCollectionTelemetryExtensions_should
{
    private readonly IServiceCollection _services = new ServiceCollection();

    [Fact]
    public void Fail_if_null_services()
    {
        var action = () => ServiceCollectionExtensions.AddTelemetry(services: null!);

        var exception = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void Register_default_telemetry_services()
    {
        _services.AddTelemetry();

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<ITelemetry<TelemetryName>>();
        Assert.NotNull(telemetry);
    }

    private class TelemetryName { }
}

public class TelemetryBuilder_should
{
    private readonly IServiceCollection _services = new ServiceCollection();

    [Fact]
    public void Fail_if_null_name()
    {
        Action[] actions = [
            () => _services.AddTelemetry(t => t.AddFor(name: null!)),
            () => _services.AddTelemetry(t => t.Add<TelemetryService>(name: null!)),
            () => _services.AddTelemetry(t => t.Add<ITelemetryService, TelemetryService>(name: null!)),
        ];

        Assert.All(actions, action =>
        {
            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("name", exception.ParamName);
        });
    }

    [Fact]
    public void Register_telemetry_services_by_name()
    {
        var name = "Name";

        _services.AddTelemetry(t => t.AddFor(name));

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
    [MemberData(nameof(OptionsData))]
    public void Configure_telemetry_services_by_name(TelemetryOptions options)
    {
        var name = "Name";
        options.Name = name;

        _services.AddTelemetry(t => t.AddFor(name).Configure(o =>
        {
            o.Version = options.Version;
            o.Tags = options.Tags;
        })); 

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetKeyedService<ITelemetry>(name);
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
        _services.AddTelemetry(t => t.AddFor<TelemetryName>());

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<ITelemetry<TelemetryName>>();
        Assert.NotNull(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.Equal(Telemetry<TelemetryName>.Name, telemetry.ActivitySource.Name);
        Assert.NotNull(telemetry.Meter);
        Assert.Equal(Telemetry<TelemetryName>.Name, telemetry.Meter.Name);
    }

    [Theory]
    [MemberData(nameof(NamedOptionsData))]
    public void Configure_telemetry_services_by_generic_type(TelemetryOptions options)
    {
        _services.AddTelemetry(t => t.AddFor<TelemetryName>().Configure(o =>
        {
            o.Version = options.Version;
            o.Tags = options.Tags;
        }));

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<ITelemetry<TelemetryName>>();
        Assert.NotNull(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }

    public static readonly TelemetryOptionsData OptionsData = [];

    public static readonly NamedTelemetryOptionsData NamedOptionsData = new(Telemetry<TelemetryName>.Name);

    private interface ITelemetryService : ITelemetry
    {
    }

    private class TelemetryService : Telemetry, ITelemetryService
    {
        public TelemetryService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions options)
            : base(loggerFactory, meterFactory, options)
        {
        }
    }

    private class TelemetryName { }
}