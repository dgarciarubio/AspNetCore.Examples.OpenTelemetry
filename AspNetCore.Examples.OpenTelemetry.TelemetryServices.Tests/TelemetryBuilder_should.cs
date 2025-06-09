using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests;

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

    [Fact]
    public void Fail_if_name_is_configured_incorrectly()
    {
        var serviceProvider = _services.AddTelemetry(t =>
        {
            t.AddFor("Name1", o => o.Name = "OtherName");
            t.AddFor<TelemetryName>(o => o.Name = "OtherName");
            t.Add<TelemetryService>("Name2", o => o.Name = "OtherName");
            t.Add<ITelemetryService, TelemetryService>("Name3", o => o.Name = "OtherName");
            t.Add<TelemetryOfTNameService>(o => o.Name = "OtherName");
            t.Add<ITelemetryOfTNameService, TelemetryOfTNameService>(o => o.Name = "OtherName");
        }).BuildServiceProvider();

        Action[] actions = [
            () => serviceProvider.GetRequiredKeyedService<ITelemetry>("Name1"),
            () => serviceProvider.GetRequiredService<ITelemetry<TelemetryName>>(),
            () => serviceProvider.GetRequiredService<TelemetryService>(),
            () => serviceProvider.GetRequiredService<ITelemetryService>(),
            () => serviceProvider.GetRequiredService<TelemetryOfTNameService>(),
            () => serviceProvider.GetRequiredService<ITelemetryOfTNameService>(),
        ];

        Assert.All(actions, action =>
        {
            Assert.Throws<InvalidOperationException>(action);
        });
    }

    [Fact]
    public void Fail_if_service_accepts_invalid_options()
    {
        Action[] actions = [
            () => _services.AddTelemetry<TelemetryServiceInvalidOptions1>("Name"),
            () => _services.AddTelemetry<ITelemetryService, TelemetryServiceInvalidOptions1>("Name"),
            () => _services.AddTelemetry<TelemetryServiceInvalidOptions2>("Name"),
            () => _services.AddTelemetry<ITelemetryService, TelemetryServiceInvalidOptions2>("Name"),
            () => _services.AddTelemetry<TelemetryServiceOfTNameInvalidOptions1>(),
            () => _services.AddTelemetry<ITelemetryOfTNameService, TelemetryServiceOfTNameInvalidOptions1>(),
            () => _services.AddTelemetry<TelemetryServiceOfTNameInvalidOptions2>(),
            () => _services.AddTelemetry<ITelemetryOfTNameService, TelemetryServiceOfTNameInvalidOptions2>(),
        ];

        Assert.All(actions, action =>
        {
            Assert.Throws<InvalidOperationException>(action);
        });
    }

    [Fact]
    public void Fail_if_configured_options_for_service_without_options()
    {
        Action[] actions = [
            () => _services.AddTelemetry<TelemetryServiceNoOptions>("Name", o => o.Version = "1.0"),
            () => _services.AddTelemetry<ITelemetryService, TelemetryServiceNoOptions>("Name", o => o.Version = "1.0"),
            () => _services.AddTelemetry<TelemetryServiceOfTNameNoOptions>(o => o.Version = "1.0"),
            () => _services.AddTelemetry<ITelemetryOfTNameService, TelemetryServiceOfTNameNoOptions>(o => o.Version = "1.0"),
        ];

        Assert.All(actions, action =>
        {
            Assert.Throws<InvalidOperationException>(action);
        });
    }

    [Fact]
    public void Return_a_telemetry_service_builder()
    {
        string name = "Name";

        var builder = _services.AddTelemetryFor(name);

        Assert.Equal(name, builder.Name);
        Assert.NotNull(builder.Telemetry);
        Assert.Same(_services, builder.Services);
    }

    [Theory]
    [MemberData(nameof(TelemetryOptionsData))]
    public void Configure_telemetry_services_by_name(TelemetryOptions options)
    {
        _services.AddTelemetryFor(options.Name, o =>
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
    [MemberData(nameof(TelemetryOptionsData))]
    public void Configure_specific_telemetry_services_by_name(TelemetryOptions options)
    {
        _services.AddTelemetry<TelemetryService>(options.Name, o =>
        {
            o.Version = options.Version;
            o.Tags = options.Tags;
        });

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<TelemetryService>();
        Assert.NotNull(telemetry);
        Assert.IsType<TelemetryService>(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }

    [Theory]
    [MemberData(nameof(TelemetryOptionsData))]
    public void Configure_specific_interfaced_telemetry_services_by_name(TelemetryOptions options)
    {
        _services.AddTelemetry<ITelemetryService, TelemetryService>(options.Name, o =>
        {
            o.Version = options.Version;
            o.Tags = options.Tags;
        });

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<ITelemetryService>();
        Assert.NotNull(telemetry);
        Assert.IsType<TelemetryService>(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }

    [Theory]
    [MemberData(nameof(TelemetryOfTNameOptionsData))]
    public void Configure_telemetry_services_by_name_type(TelemetryOptions options)
    {
        _services.AddTelemetryFor<TelemetryName>(o =>
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
    [MemberData(nameof(TelemetryOfTNameOptionsData))]
    public void Configure_specific_telemetry_services_by_name_type(TelemetryOptions options)
    {
        _services.AddTelemetry<TelemetryOfTNameService>(o =>
        {
            o.Version = options.Version;
            o.Tags = options.Tags;
        });

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<TelemetryOfTNameService>();
        var telemetryOfTName = serviceProvider.GetService<ITelemetry<TelemetryName>>();
        Assert.NotNull(telemetry);
        Assert.NotNull(telemetryOfTName);
        Assert.Same(telemetry, telemetryOfTName);
        Assert.IsType<TelemetryOfTNameService>(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }

    [Theory]
    [MemberData(nameof(TelemetryOfTNameOptionsData))]
    public void Configure_specific_interfaced_telemetry_services_by_name_type(TelemetryOptions options)
    {
        _services.AddTelemetry<ITelemetryOfTNameService, TelemetryOfTNameService>(o =>
        {
            o.Version = options.Version;
            o.Tags = options.Tags;
        });

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<ITelemetryOfTNameService>();
        var telemetryOfTName = serviceProvider.GetService<ITelemetry<TelemetryName>>();
        Assert.NotNull(telemetry);
        Assert.NotNull(telemetryOfTName);
        Assert.Same(telemetry, telemetryOfTName);
        Assert.IsType<TelemetryOfTNameService>(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(options, telemetry.ActivitySource);
        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(options, telemetry.Meter);
    }

    [Fact]
    public void Configure_version_from_configuration()
    {
        var name = Telemetry<TelemetryName>.Name;
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"Telemetry:{name}:Version"] = "1.0",
        });
        _services.AddSingleton<IConfiguration>(configuration);

        _services.AddTelemetryFor<TelemetryName>();

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<ITelemetry<TelemetryName>>();
        var expectedOptions = new TelemetryOptions { Name = name, Version = "1.0" };
        Assert.NotNull(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(expectedOptions, telemetry.ActivitySource);
        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(expectedOptions, telemetry.Meter);
    }

    [Fact]
    public void Configure_tags_from_configuration()
    {
        var name = Telemetry<TelemetryName>.Name;
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"Telemetry:{name}:Tags:Tag"] = "Value",
        });
        _services.AddSingleton<IConfiguration>(configuration);

        _services.AddTelemetryFor<TelemetryName>();

        var serviceProvider = _services.BuildServiceProvider();
        var telemetry = serviceProvider.GetService<ITelemetry<TelemetryName>>();
        var expectedOptions = new TelemetryOptions { Name = name, Tags = new Dictionary<string, object?> { ["Tag"] = "Value" } };
        Assert.NotNull(telemetry);
        Assert.NotNull(telemetry.Logger);
        Assert.NotNull(telemetry.ActivitySource);
        Assert.HasOptions(expectedOptions, telemetry.ActivitySource);
        Assert.NotNull(telemetry.Meter);
        Assert.HasOptions(expectedOptions, telemetry.Meter);
    }

    public static readonly TelemetryOptionsData TelemetryOptionsData = [];

    public static readonly TelemetryOptionsData TelemetryOfTNameOptionsData = new(Telemetry<TelemetryName>.Name);

    private interface ITelemetryService : ITelemetry
    {
    }

    private class TelemetryService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<TelemetryService> options)
        : Telemetry(loggerFactory, meterFactory, options), ITelemetryService
    {
    }

    private interface ITelemetryOfTNameService : ITelemetry<TelemetryName>
    {
    }

    private class TelemetryServiceNoOptions(ILoggerFactory loggerFactory, IMeterFactory meterFactory)
        : Telemetry(loggerFactory, meterFactory, new TelemetryOptions { Name = Name }), ITelemetryService
    {
        public static readonly string Name = "Name";
    }

    private class TelemetryOfTNameService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<TelemetryOfTNameService> options)
        : Telemetry<TelemetryName>(loggerFactory, meterFactory, options), ITelemetryOfTNameService
    {
    }

    private class TelemetryServiceOfTNameNoOptions(ILoggerFactory loggerFactory, IMeterFactory meterFactory)
        : Telemetry<TelemetryName>(loggerFactory, meterFactory), ITelemetryOfTNameService
    {
    }

    private class TelemetryServiceInvalidOptions1(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions options)
        : Telemetry(loggerFactory, meterFactory, options), ITelemetryService
    {
    }

    private class TelemetryServiceInvalidOptions2(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<ITelemetryService> options)
        : Telemetry(loggerFactory, meterFactory, options), ITelemetryService
    {
    }

    private class TelemetryServiceOfTNameInvalidOptions1(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions options)
        : Telemetry<TelemetryName>(loggerFactory, meterFactory, options), ITelemetryOfTNameService
    {
    }

    private class TelemetryServiceOfTNameInvalidOptions2(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<ITelemetryOfTNameService> options)
        : Telemetry<TelemetryName>(loggerFactory, meterFactory, options), ITelemetryOfTNameService
    {
    }

    private class TelemetryName { }
}