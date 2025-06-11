using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System.Linq.Expressions;
using System.Text;

namespace TelemetryServices.Benchmarks.Tests.Fixtures;

public class BenchmarkFixture<TBenchmark>
{
    public Summary Summary { get; } = BenchmarkRunner.Run<TBenchmark>();

    public BenchmarkReport GetReport(Expression<Action<TBenchmark>> expr)
    {
        if (expr.Body is not MethodCallExpression methodCall)
        {
            throw new InvalidOperationException("Not a method call.");
        }

        var reports = Summary.Reports.Where(r => r.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo == methodCall.Method.Name).ToArray();
        return reports switch
        {
            { Length: 1 } => reports[0],
            { Length: 0 } => throw new InvalidOperationException("Report not found."),
            _ => throw new InvalidOperationException("Multiple reports found."),
        };
    }

    public double GetAverageExecutionTimeNs(Expression<Action<TBenchmark>> expr)
    {
        return GetReport(expr)
            .GetResultRuns()
            .Average(r => r.Nanoseconds / r.Operations);
    }
}
