using AspNetCore.Examples.OpenTelemetry.TelemetryServices.Benchmarks;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var settings = configuration.GetSection("Benchmarks").Get<Dictionary<string, BenchmarkSettings>>() ?? new();

var benchmarks = new Type[] {
    typeof(DependencyInjectionBenchmarks),
    typeof(LoggerBenchmarks),
    typeof(ActivitySourceBenchmarks),
    typeof(MeterBenchmarks),
};
var benchmarkRuns = benchmarks.ToDictionary<Type, string, Action>(
    t => t.Name,
    t => () => BenchmarkRunner.Run(t)
);

foreach (var run in benchmarkRuns)
{
    var settingsKey = run.Key;
    var runBenchmark = run.Value;

    if (settings.TryGetValue(settingsKey, out var benchmarkSettings) &&
        benchmarkSettings.Run)
    {
        runBenchmark();
    }
}

public class BenchmarkSettings
{
    public bool Run { get; set; }
}