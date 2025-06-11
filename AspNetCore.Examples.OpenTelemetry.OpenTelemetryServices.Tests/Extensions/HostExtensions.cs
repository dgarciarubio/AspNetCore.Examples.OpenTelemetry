#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.Hosting;
#pragma warning restore IDE0130 // Namespace does not match folder structure

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
