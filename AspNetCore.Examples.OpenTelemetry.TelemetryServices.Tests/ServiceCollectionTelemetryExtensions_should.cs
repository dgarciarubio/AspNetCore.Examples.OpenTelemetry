using Microsoft.Extensions.DependencyInjection;

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
