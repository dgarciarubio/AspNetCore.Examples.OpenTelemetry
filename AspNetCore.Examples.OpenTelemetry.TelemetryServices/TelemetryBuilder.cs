using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;
using System.Reflection;
using static System.Collections.Specialized.BitVector32;

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
            var iTelemetryOfTName = GetImplementITelemetryOfTNameInterface(typeof(TService));
            if (iTelemetryOfTName is not null)
            {
                Services.TryAddSingleton(iTelemetryOfTName, sp => sp.GetRequiredService<TService>());
            }

            return new TelemetryServiceBuilder(this, name);
        }

        private void AddTelemetryOptions<TService>(string name, Action<TelemetryOptions<TService>>? configureOptions)
            where TService : ITelemetry
        {
            Services.AddOptions<TelemetryOptions<TService>>(name)
                .Configure<IServiceProvider>((options, sp) =>
                {
                    BindFromConfiguration(options, sp);

                    options.Name = name;
                    configureOptions?.Invoke(options);
                    if (options.Name != name)
                    {
                        throw new InvalidOperationException("The configured telemetry options do not have the expected name.");
                    }
                });

            Services.TryAddTransient(sp => sp
                .GetRequiredService<IOptionsMonitor<TelemetryOptions<TService>>>()
                .Get(name)
            );

            void BindFromConfiguration(TelemetryOptions<TService> options, IServiceProvider serviceProvider)
            {
                var section = serviceProvider.GetService<IConfiguration>()?
                    .GetSection("Telemetry")
                    .GetSection(name);

                if (section is not null)
                {
                    section.Bind(options);
                    options.Tags = section
                        .GetSection(nameof(TelemetryOptions.Tags))
                        .Get<Dictionary<string, object?>>();
                    BindTagsFromConfiguration(options.Logger, section.GetSection(nameof(TelemetryOptions.Logger)));
                    BindTagsFromConfiguration(options.ActivitySource, section.GetSection(nameof(TelemetryOptions.ActivitySource)));
                    BindTagsFromConfiguration(options.Meter, section.GetSection(nameof(TelemetryOptions.Meter)));
                }
            }

            void BindTagsFromConfiguration(TelemetryElementOptions options, IConfigurationSection configuration)
            {
                var tagsSection = configuration
                    .GetSection(nameof(TelemetryElementOptions.Tags));
                if (tagsSection.Exists())
                {
                    options.Tags = tagsSection.Get<Dictionary<string, object?>>();
                }
            }
        }

        private static Type GetTelemetryNameType<TService>()
        {
            var telemetryOfTNameType = GetBaseTelemetryOfTNameType(typeof(TService));
            return telemetryOfTNameType?.GenericTypeArguments.Single() ??
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

        private static Type? GetImplementITelemetryOfTNameInterface(Type type)
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

        private static Type? GetBaseTelemetryOfTNameType(Type? type)
        {
            if (type is null)
            {
                return null;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Telemetry<>))
            {
                return type;
            }
            return GetBaseTelemetryOfTNameType(type.BaseType);
        }
    }
}



