using AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Collections;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests;

public class OptionsEnrichedLogger_should
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMeterFactory _meterFactory;

    public OptionsEnrichedLogger_should()
    {
        _logger = Substitute.For<ILogger>();
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(_logger);
        _meterFactory = Substitute.For<IMeterFactory>();
        _meterFactory.Create(Arg.Any<MeterOptions>()).Returns((callInfo) =>
        {
            var options = callInfo.Arg<MeterOptions>();
            return new Meter(options);
        });
    }

    [Fact]
    public void Not_be_used_if_options_not_contain_enrichment_data()
    {
        var options = new TelemetryOptions { Name = "Name" };
        ILogger? loggerCreated = null;
        using var listener = new LoggingListener();
        listener.LoggerCreated += (logger) => loggerCreated = logger;

        using var telemetry = new Telemetry(listener, _meterFactory, options);

        Assert.NotNull(loggerCreated);
        Assert.Same(loggerCreated, telemetry.Logger);
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
        ILogger? loggerCreated = null;
        using var listener = new LoggingListener();
        listener.LoggerCreated += (logger) => loggerCreated = logger;

        using var telemetry = new Telemetry(listener, _meterFactory, options);

        Assert.NotNull(loggerCreated);
        Assert.Same(loggerCreated, telemetry.Logger);
    }

    [Fact]
    public void Enrich_logs_with_options_version()
    {
        var options = new TelemetryOptions { Name = "Name", Version = "1.0" };
        IEnumerable<KeyValuePair<string, object?>>? loggedState = null;
        using var listener = new LoggingListener();
        listener.Logged += (data) => loggedState = data.State as IEnumerable<KeyValuePair<string, object?>>;

        using var telemetry = new Telemetry(listener, _meterFactory, options);
        telemetry.Logger.LogDebug("Log");

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
        IEnumerable<KeyValuePair<string, object?>>? loggedState = null;
        using var listener = new LoggingListener();
        listener.Logged += (data) => loggedState = data.State as IEnumerable<KeyValuePair<string, object?>>;

        using var telemetry = new Telemetry(listener, _meterFactory, options);
        telemetry.Logger.LogDebug("Log");

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
        IEnumerable<KeyValuePair<string, object?>>? loggedState = null;
        using var listener = new LoggingListener();
        listener.Logged += (data) => loggedState = data.State as IEnumerable<KeyValuePair<string, object?>>;

        using var telemetry = new Telemetry(listener, _meterFactory, options);
        telemetry.Logger.Log(LogLevel.Debug,
            state: originalState,
            eventId: 0,
            exception: null,
            formatter: (_, _) => "Log"
        );

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
        IEnumerable<KeyValuePair<string, object?>>? loggedState = null;
        using var listener = new LoggingListener();
        listener.Logged += (data) => loggedState = data.State as IEnumerable<KeyValuePair<string, object?>>;

        using var telemetry = new Telemetry(listener, _meterFactory, options);
        telemetry.Logger.Log(LogLevel.Debug,
            state: originalState,
            eventId: 0,
            exception: null,
            formatter: (_, _) => "Log"
        );

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
        IEnumerable<KeyValuePair<string, object?>>? loggedState = null;
        using var listener = new LoggingListener();
        listener.Logged += (data) => loggedState = data.State as IEnumerable<KeyValuePair<string, object?>>;

        using var telemetry = new Telemetry(listener, _meterFactory, options);
        telemetry.Logger.Log<object?>(LogLevel.Debug,
            state: null,
            eventId: 0,
            exception: null,
            formatter: (_, _) => "Log"
        );
        telemetry.Logger.Log<int?>(LogLevel.Debug,
            state: null,
            eventId: 0,
            exception: null,
            formatter: (_, _) => "Log"
        );

        Assert.NotNull(loggedState);
        Assert.DoesNotContain(loggedState, s => s.Key == "State");
    }

    [Theory]
    [ClassData(typeof(StandardLogStatesData))]
    public void Produce_a_dictionary_like_state_in_defined_order(IEnumerable<KeyValuePair<string, object?>> originalState)
    {
        var options = new TelemetryOptions { Name = "Name", Version = "1.0", Tags = new Dictionary<string, object?>() { ["Tag"] = "TagValue" } };
        IReadOnlyList<KeyValuePair<string, object?>>? loggedState = null;
        IEnumerator? loggedStateEnumerator = null;
        using var listener = new LoggingListener();
        listener.Logged += (data) =>
        {
            loggedState = data.State as IReadOnlyList<KeyValuePair<string, object?>>;
            loggedStateEnumerator = (data.State as IEnumerable)?.GetEnumerator();
        };

        using var telemetry = new Telemetry(listener, _meterFactory, options);
        telemetry.Logger.Log(LogLevel.Debug,
            state: originalState,
            eventId: 0,
            exception: null,
            formatter: (_, _) => "Log"
        );

        Assert.NotNull(loggedState);
        Assert.NotNull(loggedStateEnumerator);
        Assert.Equal(1 + options.Tags.Count() + originalState.Count(), loggedState.Count);
        Assert.All(options.Tags, (tag, i) =>
        {
            Assert.Equal(tag, loggedState[i + 1]);
        });
        Assert.All(originalState, (s, i) =>
        {
            Assert.Equal(s, loggedState[i + 1 + options.Tags.Count()]);
        });
    }

    [Fact]
    public void Forward_scopes_to_underlying_logger()
    {
        var options = new TelemetryOptions { Name = "Name", Version = "1.0" };
        Dictionary<string, object?>? begunScope = null;
        using var scopeDisposable = new LoggingScopeDisposable();
        using var listener = new LoggingListener();
        listener.BegunScope += (state) =>
        {
            begunScope = state as Dictionary<string, object?>;
            return scopeDisposable;
        };

        using var telemetry = new Telemetry(listener, _meterFactory, options);
        using var beginScopeResponse = telemetry.Logger.BeginScope(new Dictionary<string, object?> { ["ScopeTag"] = "ScopeTagValue" });

        Assert.NotNull(begunScope);
        Assert.NotEmpty(begunScope);
        Assert.Same(scopeDisposable, beginScopeResponse);
    }

    [Theory]
    [InlineData(LogLevel.Debug, true)]
    [InlineData(LogLevel.Debug, false)]
    [InlineData(LogLevel.Information, true)]
    [InlineData(LogLevel.Information, false)]
    public void Forward_log_level_enabled_to_underlying_logger(LogLevel logLevel, bool isEnabled)
    {
        var options = new TelemetryOptions { Name = "Name", Version = "1.0" };
        LogLevel? logLevelEnabledCalled = null;
        using var listener = new LoggingListener();
        listener.IsLevelEnabledCalled += (logLevel) =>
        {
            logLevelEnabledCalled = logLevel;
            return isEnabled;
        };

        using var telemetry = new Telemetry(listener, _meterFactory, options);
        var isEnabledResponse = telemetry.Logger.IsEnabled(logLevel);

        Assert.NotNull(logLevelEnabledCalled);
        Assert.Equal(logLevel, logLevelEnabledCalled);
        Assert.Equal(isEnabled, isEnabledResponse);
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

    private class LoggingScopeDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
