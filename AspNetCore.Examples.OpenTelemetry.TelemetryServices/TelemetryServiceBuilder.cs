using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices
{
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



