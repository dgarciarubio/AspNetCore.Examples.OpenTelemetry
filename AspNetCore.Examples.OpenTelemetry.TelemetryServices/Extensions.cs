using AspNetCore.Examples.OpenTelemetry.TelemetryServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;
using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelemetry(this IServiceCollection services, Action<TelemetryBuilder>? configure = null)
        {
            var builder = services.AddTelemetry();
            configure?.Invoke(builder);
            return services;
        }

        public static TelemetryServiceBuilder AddTelemetryFor(this IServiceCollection services, string name, Action<TelemetryOptions<ITelemetry>>? configureOptions = null)
            => services.AddTelemetry().AddFor(name, configureOptions);

        public static TelemetryServiceBuilder AddTelemetryFor<TTelemetryName>(this IServiceCollection services, Action<TelemetryOptions<ITelemetry<TTelemetryName>>>? configureOptions = null)
            => services.AddTelemetry().AddFor(configureOptions);

        public static TelemetryServiceBuilder AddTelemetry<TService>(this IServiceCollection services, Action<TelemetryOptions<TService>>? configureOptions = null)
            where TService : Telemetry
            => services.AddTelemetry().Add(configureOptions);

        public static TelemetryServiceBuilder AddTelemetry<TService, TImplementation>(this IServiceCollection services, Action<TelemetryOptions<TImplementation>>? configureOptions = null)
            where TService : class, ITelemetry
            where TImplementation : Telemetry, TService
            => services.AddTelemetry().Add<TService, TImplementation>(configureOptions);

        public static TelemetryServiceBuilder AddTelemetry<TService>(this IServiceCollection services, string name, Action<TelemetryOptions<TService>>? configureOptions = null)
            where TService : Telemetry
            => services.AddTelemetry().Add(name, configureOptions);

        public static TelemetryServiceBuilder AddTelemetry<TService, TImplementation>(this IServiceCollection services, string name, Action<TelemetryOptions<TImplementation>>? configureOptions = null)
            where TService : class, ITelemetry
            where TImplementation : Telemetry, TService
            => services.AddTelemetry().Add<TService, TImplementation>(name, configureOptions);

        private static TelemetryBuilder AddTelemetry(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            services.AddLogging();
            services.AddMetrics();
            services.TryAddSingleton(typeof(ITelemetry<>), typeof(Telemetry<>));

            return new TelemetryBuilder(services);
        }

    }
}

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices
{
    public class TelemetryBuilder
    {
        internal TelemetryBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public TelemetryServiceBuilder AddFor(string name, Action<TelemetryOptions<ITelemetry>>? configureOptions = null)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));

            AddTelemetryOptions(name, configureOptions);
            Services.TryAddKeyedSingleton<ITelemetry>(name, (sp, key) =>
            {
                var name = (string)key!;
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var meterFactory = sp.GetRequiredService<IMeterFactory>();
                var options = sp
                    .GetRequiredService<IOptionsMonitor<TelemetryOptions<ITelemetry>>>()
                    .Get(name);
                return new Telemetry(loggerFactory, meterFactory, options);
            });

            return new TelemetryServiceBuilder(this, name);
        }

        public TelemetryServiceBuilder AddFor<TTelemetryName>(Action<TelemetryOptions<ITelemetry<TTelemetryName>>>? configureOptions = null)
        {
            var name = Telemetry<TTelemetryName>.Name;

            AddTelemetryOptions(name, configureOptions);
            Services.TryAddSingleton(typeof(ITelemetry<TTelemetryName>), sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var meterFactory = sp.GetRequiredService<IMeterFactory>();
                var options = sp.GetService<TelemetryOptions<ITelemetry<TTelemetryName>>>();
                return new Telemetry<TTelemetryName>(loggerFactory, meterFactory, options);
            });

            return new TelemetryServiceBuilder(this, name);
        }

        public TelemetryServiceBuilder Add<TService>(Action<TelemetryOptions<TService>>? configureOptions = null)
            where TService : Telemetry
        {
            return Add<TService, TService>(configureOptions);
        }

        public TelemetryServiceBuilder Add<TService, TImplementation>(Action<TelemetryOptions<TImplementation>>? configureOptions = null)
            where TService : class, ITelemetry
            where TImplementation : Telemetry, TService
        {
            var telemetryNameType = GetTelemetryNameType<TImplementation>();
            var name = TelemetryNameHelper.GetName(telemetryNameType);
            return Add<TService, TImplementation>(name, configureOptions);
        }

        public TelemetryServiceBuilder Add<TService>(string name, Action<TelemetryOptions<TService>>? configureOptions = null)
            where TService : Telemetry
        {
            return Add<TService, TService>(name, configureOptions);
        }

        public TelemetryServiceBuilder Add<TService, TImplementation>(string name, Action<TelemetryOptions<TImplementation>>? configureOptions = null)
            where TService : class, ITelemetry
            where TImplementation : Telemetry, TService
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));

            ValidateConstructors(configureOptions, out bool acceptsOptions);
            if (acceptsOptions)
            {
                AddTelemetryOptions(name, configureOptions);
            }

            Services.TryAddSingleton<TService, TImplementation>();
            var genericTelemetryInterface = GetImplementedGenericTelemetryInterface(typeof(TService));
            if (genericTelemetryInterface is not null)
            {
                Services.TryAddSingleton(genericTelemetryInterface, sp => sp.GetRequiredService<TService>());
            }

            return new TelemetryServiceBuilder(this, name);
        }

        private void AddTelemetryOptions<TService>(string name, Action<TelemetryOptions<TService>>? configureOptions)
            where TService : ITelemetry
        {
            Services.AddOptions<TelemetryOptions<TService>>(name)
                .Configure<IServiceProvider>((options, sp) =>
                {
                    sp.GetService<IConfiguration>()?
                        .GetSection("Telemetry")
                        .GetSection(name)?
                        .Bind(options);

                    options.Name = name;
                    configureOptions?.Invoke(options);
                    if (options.Name != name)
                    {
                        throw new InvalidOperationException("The configured telemetry options do not have the expected name.");
                    }
                });

            Services.TryAddSingleton(sp => sp
                .GetRequiredService<IOptionsMonitor<TelemetryOptions<TService>>>()
                .Get(name)
            );
        }

        private static Type GetTelemetryNameType<TService>()
        {
            var genericTelemetryType = GetBaseGenericTelemetryType(typeof(TService));
            return genericTelemetryType?.GenericTypeArguments.Single() ??
                throw new InvalidOperationException($"The specified telemetry service is not derived from a generic {nameof(Telemetry<>)}.");
        }

        private static void ValidateConstructors<TService>(Action<TelemetryOptions<TService>>? configureOptions, out bool acceptsOptions)
            where TService : Telemetry
        {
            var constructors = typeof(TService).GetConstructors();
            var acceptsValidOptions = constructors.Any(c => c.GetParameters().Any(IsValidOptions));
            var acceptsInvalidOptions = constructors.Any(c => c.GetParameters().Any(IsInvalidOptions));
            if (acceptsInvalidOptions)
            {
                throw new InvalidOperationException($"The specified telemetry service accepts an unexpected options type. Inject a generic {nameof(TelemetryOptions<>)} object of the telemetry service type itself.");
            }
            if (!acceptsValidOptions && configureOptions is not null)
            {
                throw new InvalidOperationException("The specified telemetry service does not accept options. It cannot be configured.");
            }

            acceptsOptions = acceptsValidOptions;

            bool IsValidOptions(ParameterInfo parameter)
            {
                return parameter.ParameterType == typeof(TelemetryOptions<TService>);
            }

            bool IsInvalidOptions(ParameterInfo parameter)
            {
                return parameter.ParameterType == typeof(TelemetryOptions) ||
                    parameter.ParameterType.IsGenericType &&
                    parameter.ParameterType.GetGenericTypeDefinition() == typeof(TelemetryOptions<>) &&
                    parameter.ParameterType.GenericTypeArguments.Single() != typeof(TService);
            }
        }

        private static Type? GetImplementedGenericTelemetryInterface(Type type)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITelemetry<>))
                .ToArray();

            return interfaces switch
            {
                { Length: 1 } => interfaces.Single(),
                _ => null,
            };
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

    public class TelemetryServiceBuilder
    {
        internal TelemetryServiceBuilder(TelemetryBuilder telemetry, string name)
        {
            Telemetry = telemetry;
            Name = name;
        }

        public TelemetryBuilder Telemetry { get; }
        public IServiceCollection Services => Telemetry.Services;
        public string Name { get; }
    }
}



