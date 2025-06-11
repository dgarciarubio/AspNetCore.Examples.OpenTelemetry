using Microsoft.Extensions.Hosting;

namespace AspNetCore.Examples.OpenTelemetry.OpenTelemetryServices.Tests.Extensions;

internal static class HostApplicationBuilderExtensions
{
    public static async Task RunInHost(this HostApplicationBuilder builder, Func<IHost, Task> action)
    {
        using var host = builder.Build();
        using var cts = new CancellationTokenSource();
        _ = host.RunAsync(cts.Token);

        await action(host);

        cts.Cancel();
    }

    public static void RunInHost(this HostApplicationBuilder builder, Action<IHost> action)
    {
        using var host = builder.Build();
        using var cts = new CancellationTokenSource();
        _ = host.RunAsync(cts.Token);

        action(host);

        cts.Cancel();
    }
}
