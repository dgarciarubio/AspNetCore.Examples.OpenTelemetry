using AspNetCore.Examples.OpenTelemetry.TelemetryServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;
using System.Xml.Linq;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelemetry(this IServiceCollection services, Action<TelemetryBuilder>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            services.AddLogging();
            services.AddMetrics();

            services.TryAddSingleton(typeof(ITelemetry<>), typeof(Telemetry<>));

            var builder = new TelemetryBuilder(services);
            configure?.Invoke(builder);

            return services;
        }
    }
}

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices
{
    public class TelemetryBuilder
    {
        public IServiceCollection Services { get; }

        internal TelemetryBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public TelemetryServiceBuilder<Telemetry> AddFor(string name)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));

            Services.TryAddKeyedSingleton<ITelemetry>(name, (sp, key) =>
            {
                var name = (string)key!;
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var meterFactory = sp.GetRequiredService<IMeterFactory>();
                var options = sp.GetRequiredService<IOptionsMonitor<TelemetryOptions<Telemetry>>>().Get(name);
                return new Telemetry(loggerFactory, meterFactory, options);
            });
            return new TelemetryServiceBuilder<Telemetry>(Services, name, useNamedOptions: true);
        }

        public TelemetryServiceBuilder<Telemetry<TTelemetryName>> AddFor<TTelemetryName>()
        {
            Services.TryAddSingleton(typeof(ITelemetry<TTelemetryName>), sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var meterFactory = sp.GetRequiredService<IMeterFactory>();
                var options = sp.GetRequiredService<TelemetryOptions<Telemetry<TTelemetryName>>>();
                return new Telemetry<TTelemetryName>(loggerFactory, meterFactory, options);
            });
            return new TelemetryServiceBuilder<Telemetry<TTelemetryName>>(Services, Telemetry<TTelemetryName>.Name);
        }

        public TelemetryServiceBuilder<TService> Add<TService>()
            where TService : class, ITelemetry
        {
            return Add<TService, TService>();
        }

        public TelemetryServiceBuilder<TImplementation> Add<TService, TImplementation>()
            where TService : class, ITelemetry
            where TImplementation : class, TService
        {
            var genericTelemetryType = GetBaseGenericTelemetryType(typeof(TImplementation))
                ?? throw new InvalidOperationException("Cannot determine the telemetry name from the specified telemetry service");

            var telemetryNameType = genericTelemetryType.GenericTypeArguments.Single();
            var name = TelemetryNameHelper.GetName(telemetryNameType);
            return Add<TService, TImplementation>(name);
        }

        public TelemetryServiceBuilder<TService> Add<TService>(string name)
            where TService : class, ITelemetry
        {
            return Add<TService, TService>(name);
        }

        public TelemetryServiceBuilder<TImplementation> Add<TService, TImplementation>(string name)
            where TService : class, ITelemetry
            where TImplementation : class, TService
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));

            Services.TryAddSingleton<TService, TImplementation>();

            foreach (var genericTelemetryInterface in GetImplementedGenericTelemetryInterfaces(typeof(TService)))
            {
                Services.TryAddSingleton(genericTelemetryInterface, sp => sp.GetRequiredService<TService>());
            }

            return new TelemetryServiceBuilder<TImplementation>(Services, name);
        }

        private static IEnumerable<Type> GetImplementedGenericTelemetryInterfaces(Type type)
        {
            return type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITelemetry<>));
        }

        private static Type? GetBaseGenericTelemetryType(Type? type)
        {
            if (type is null)
            {
                return null;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Telemetry<>))
            {
                return type;
            }
            return GetBaseGenericTelemetryType(type.BaseType);
        }
    }

    public class TelemetryServiceBuilder<TService>
        where TService : ITelemetry
    {
        private readonly string? _optionsName;

        internal TelemetryServiceBuilder(IServiceCollection services, string telemetryName, bool useNamedOptions = false)
        {
            Services = services;
            Name = telemetryName;
            _optionsName = useNamedOptions ? telemetryName : null;

            Configure(options => { });
            if (!useNamedOptions)
            {
                Services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<TelemetryOptions<TService>>>().Value);
            }
        }

        public IServiceCollection Services { get; }
        public string Name { get; }

        public TelemetryServiceBuilder<TService> Configure(Action<TelemetryOptions<TService>> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            Services.Configure<TelemetryOptions<TService>>(_optionsName, options =>
            {
                configureOptions(options);
                options.Name = Name;
            });
            return this;
        }
    }
}



