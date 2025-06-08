using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

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
    private readonly IServiceCollection _services = new ServiceCollection().AddTelemetry();

    [Fact]
    public void Fail_if_null_name()
    {
        Action[] actions = [
            () => _services.AddTelemetryFor(name: null!),
            () => _services.AddTelemetry<TelemetryService>(name: null!),
            () => _services.AddTelemetry<ITelemetryService, TelemetryService>(name: null!),
        ];

        Assert.All(actions, action =>
        {
            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("name", exception.ParamName);
        });
    }

    [Fact]
    public void Fail_if_name_cannot_be_automatically_determined()
    {
        Action[] actions = [
            () => _services.AddTelemetry<TelemetryService>(),
            () => _services.AddTelemetry<ITelemetryService, TelemetryService>(),
        ];

        Assert.All(actions, action =>
        {
            Assert.Throws<InvalidOperationException>(action);
        });
    }

    [Theory]
    [MemberData(nameof(OptionsData))]
    public void Configure_telemetry_services_by_name(TelemetryOptions options)
    {
        _services.AddTelemetryFor(options.Name).Configure(o =>
        {
            o.Version = options.Version;
            o.Tags = options.Tags;
        });

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetKeyedService<ITelemetry>(options.Name);
        Assert.NotNull(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }

    [Theory]
    [MemberData(nameof(OptionsData))]
    public void Configure_specific_telemetry_services_by_name(TelemetryOptions options)
    {
        _services.AddTelemetry<TelemetryService>(options.Name).Configure(o =>
        {
            o.Version = options.Version;
            o.Tags = options.Tags;
        });

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<TelemetryService>();
        Assert.NotNull(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }

    [Theory]
    [MemberData(nameof(OptionsData))]
    public void Configure_specific_interfaced_telemetry_services_by_name(TelemetryOptions options)
    {
        _services.AddTelemetry<ITelemetryService, TelemetryService>(options.Name).Configure(o =>
        {
            o.Version = options.Version;
            o.Tags = options.Tags;
        });

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<ITelemetryService>();
        Assert.NotNull(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }

    [Theory]
    [MemberData(nameof(NamedOptionsData))]
    public void Configure_telemetry_services_by_generic_type(TelemetryOptions options)
    {
        _services.AddTelemetryFor<TelemetryName>().Configure(o =>
        {
            o.Version = options.Version;
            o.Tags = options.Tags;
        });

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<ITelemetry<TelemetryName>>();
        Assert.NotNull(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }

    [Theory]
    [MemberData(nameof(NamedOptionsData))]
    public void Configure_specific_telemetry_services_by_generic_type(TelemetryOptions options)
    {
        _services.AddTelemetry<GenericTelemetryService>().Configure(o =>
        {
            o.Version = options.Version;
            o.Tags = options.Tags;
        });

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<GenericTelemetryService>();
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

    [Theory]
    [MemberData(nameof(NamedOptionsData))]
    public void Configure_specific_interfaced_telemetry_services_by_generic_type(TelemetryOptions options)
    {
        _services.AddTelemetry<IGenericTelemetryService, GenericTelemetryService>().Configure(o =>
        {
            o.Version = options.Version;
            o.Tags = options.Tags;
        });

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<IGenericTelemetryService>();
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


    public static readonly TelemetryOptionsData OptionsData = [];

    public static readonly NamedTelemetryOptionsData NamedOptionsData = new(Telemetry<TelemetryName>.Name);

    private interface ITelemetryService : ITelemetry
    {
    }

    private class TelemetryService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<TelemetryService> options) 
        : Telemetry(loggerFactory, meterFactory, options), ITelemetryService
    {
    }

    private interface IGenericTelemetryService : ITelemetry<TelemetryName>
    {
    }

    private class GenericTelemetryService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<GenericTelemetryService> options)
        : Telemetry<TelemetryName>(loggerFactory, meterFactory, options), IGenericTelemetryService
    {
    }

    private class TelemetryName { }
}