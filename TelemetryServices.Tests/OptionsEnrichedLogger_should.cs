using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Collections;
using System.Diagnostics.Metrics;
using TelemetryServices.Tests.TestDoubles;

namespace TelemetryServices.Tests;

public sealed class OptionsEnrichedLogger_should : IDisposable
{
    private readonly IMeterFactory _meterFactory;
    private readonly LoggingListener _loggingListener;

    public OptionsEnrichedLogger_should()
    {
        _meterFactory = Substitute.For<IMeterFactory>();
        _meterFactory.Create(Arg.Any<MeterOptions>()).Returns((callInfo) =>
        {
            var options = callInfo.Arg<MeterOptions>();
            return new Meter(options);
        });
        _loggingListener = new LoggingListener();
    }

    [Fact]
    public void Not_be_used_if_options_not_contain_enrichment_data()
    {
        var options = new TelemetryOptions { Name = "Name" };

        using var telemetry = new Telemetry(_loggingListener, _meterFactory, options);

        var logger = _loggingListener.Loggers.FirstOrDefault();
        Assert.NotNull(logger);
        Assert.Same(logger, telemetry.Logger);
    }

    [Fact]
    public void Not_be_used_if_logger_options_not_contain_enrichment_data()
    {
        var options = new TelemetryOptions
        {
            Name = "Name",
            Version = "1.0",
            Tags = new Dictionary<string, object?>() { ["Tag"] = "TagValue" },
            Logger =
            {
                Version = null,
                Tags = null,
            }
        };

        using var telemetry = new Telemetry(_loggingListener, _meterFactory, options);

        var logger = _loggingListener.Loggers.FirstOrDefault();
        Assert.NotNull(logger);
        Assert.Same(logger, telemetry.Logger);
    }

    [Fact]
    public void Enrich_logs_with_options_version()
    {
        var options = new TelemetryOptions { Name = "Name", Version = "1.0" };

        using var telemetry = new Telemetry(_loggingListener, _meterFactory, options);
        telemetry.Logger.LogDebug("Log");

        var loggedState = _loggingListener.FormattedLogs.FirstOrDefault()?.State;
        Assert.NotNull(loggedState);
        Assert.Contains(loggedState, s =>
            s.Key == nameof(TelemetryOptions.Version) &&
            s.Value?.ToString() == options.Version
        );
    }

    [Fact]
    public void Enrich_logs_with_options_tags()
    {
        var options = new TelemetryOptions { Name = "Name", Tags = new Dictionary<string, object?>() { ["Tag"] = "TagValue" } };

        using var telemetry = new Telemetry(_loggingListener, _meterFactory, options);
        telemetry.Logger.LogDebug("Log");

        var loggedState = _loggingListener.FormattedLogs.FirstOrDefault()?.State;
        Assert.NotNull(loggedState);
        Assert.All(options.Tags, optionsTag =>
        {
            Assert.Contains(loggedState, s =>
                s.Key == optionsTag.Key &&
                s.Value == optionsTag.Value
            );
        });
    }

    [Theory]
    [ClassData(typeof(StandardLogStatesData))]
    public void Preserve_original_standard_log_state(IEnumerable<KeyValuePair<string, object?>> originalState)
    {
        var options = new TelemetryOptions { Name = "Name", Version = "1.0" };

        using var telemetry = new Telemetry(_loggingListener, _meterFactory, options);
        telemetry.Logger.Log(LogLevel.Debug,
            state: originalState,
            eventId: 0,
            exception: null,
            formatter: (_, _) => "Log"
        );

        var loggedState = _loggingListener.FormattedLogs.FirstOrDefault()?.State;
        Assert.NotNull(loggedState);
        Assert.All(originalState, (s, i) =>
        {
            Assert.Contains(loggedState, ss =>
                ss.Key == s.Key &&
                ss.Value == s.Value
            );
        });
    }

    [Theory]
    [ClassData(typeof(NonStandardLogStatesData))]
    public void Preserve_original_non_standard_log_state(object originalState)
    {
        var options = new TelemetryOptions { Name = "Name", Version = "1.0" };

        using var telemetry = new Telemetry(_loggingListener, _meterFactory, options);
        telemetry.Logger.Log(LogLevel.Debug,
            state: originalState,
            eventId: 0,
            exception: null,
            formatter: (_, _) => "Log"
        );

        var loggedState = _loggingListener.FormattedLogs.FirstOrDefault()?.State;
        Assert.NotNull(loggedState);
        Assert.Contains(loggedState, ss =>
            ss.Key == "State" &&
            ss.Value == originalState
        );
    }

    [Fact]
    public void Not_preserve_original_null_log_state()
    {
        var options = new TelemetryOptions { Name = "Name", Version = "1.0" };

        using var telemetry = new Telemetry(_loggingListener, _meterFactory, options);
        telemetry.Logger.Log<object?>(LogLevel.Debug,
            state: null,
            eventId: 0,
            exception: null,
            formatter: (_, _) => "Log"
        );

        var loggedState = _loggingListener.FormattedLogs.FirstOrDefault()?.State;
        Assert.NotNull(loggedState);
        Assert.DoesNotContain(loggedState, s => s.Key == "State");
    }

    [Theory]
    [ClassData(typeof(StandardLogStatesData))]
    public void Produce_a_state_in_defined_order(IEnumerable<KeyValuePair<string, object?>> originalState)
    {
        var options = new TelemetryOptions { Name = "Name", Version = "1.0", Tags = new Dictionary<string, object?>() { ["Tag"] = "TagValue" } };

        using var telemetry = new Telemetry(_loggingListener, _meterFactory, options);
        telemetry.Logger.Log(LogLevel.Debug,
            state: originalState,
            eventId: 0,
            exception: null,
            formatter: (_, _) => "Log"
        );

        var loggedState = _loggingListener.FormattedLogs.FirstOrDefault()?.State;
        var loggedStateList = loggedState as IReadOnlyList<KeyValuePair<string, object?>>;
        var loggedStateEnumerator = (loggedState as IEnumerable)?.GetEnumerator();
        Assert.NotNull(loggedStateList);
        Assert.NotNull(loggedStateEnumerator);
        Assert.Equal(1 + options.Tags.Count() + originalState.Count(), loggedStateList.Count);
        Assert.All(options.Tags, (tag, i) =>
        {
            Assert.Equal(tag, loggedStateList[i + 1]);
        });
        Assert.All(originalState, (s, i) =>
        {
            Assert.Equal(s, loggedStateList[i + 1 + options.Tags.Count()]);
        });
    }

    [Fact]
    public void Forward_scopes_to_underlying_logger()
    {
        var options = new TelemetryOptions { Name = "Name", Version = "1.0" };
        var scope = new Dictionary<string, object?> { ["ScopeTag"] = "ScopeTagValue" };

        using var telemetry = new Telemetry(_loggingListener, _meterFactory, options);
        using var scopeDisposable = telemetry.Logger.BeginScope(scope);

        var begunScope = _loggingListener.TagListScopes.FirstOrDefault();
        Assert.NotNull(begunScope);
        Assert.Equal(scope.Count, begunScope.Count());
        Assert.All(scope, s =>
        {
            Assert.Contains(begunScope, ss =>
                ss.Key == s.Key &&
                ss.Value == s.Value
            );
        });
        Assert.Same(LoggingListener.NullScopeDisposable.Instance, scopeDisposable);
    }

    [Theory]
    [InlineData(LogLevel.Information, LogLevel.Information, true)]
    [InlineData(LogLevel.Information, LogLevel.Warning, true)]
    [InlineData(LogLevel.Warning, LogLevel.Information, false)]
    public void Forward_log_level_enabled_to_underlying_logger(LogLevel minLogLevel, LogLevel logLevel, bool shouldBeEnabled)
    {
        var options = new TelemetryOptions { Name = "Name", Version = "1.0" };
        _loggingListener.MinLogLevel = minLogLevel;

        using var telemetry = new Telemetry(_loggingListener, _meterFactory, options);
        var isEnabled = telemetry.Logger.IsEnabled(logLevel);

        Assert.Equal(isEnabled, shouldBeEnabled);
    }

    public void Dispose()
    {
        _loggingListener.Dispose();
    }

    public class StandardLogStatesData : TheoryData<IEnumerable<KeyValuePair<string, object?>>>
    {
        public StandardLogStatesData()
        {
            Add(GetState());
            Add(GetState().ToArray());
            Add(GetState().ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }

        private static IEnumerable<KeyValuePair<string, object?>> GetState()
        {
            yield return new("StateTag1", "TagValue1");
            yield return new("StateTag2", "TagValue2");
        }
    }

    public class NonStandardLogStatesData : TheoryData<object>
    {
        public NonStandardLogStatesData()
        {
            Add(1);
            Add("State");
            Add(new DateTime(2025, 01, 01));
        }
    }
}
