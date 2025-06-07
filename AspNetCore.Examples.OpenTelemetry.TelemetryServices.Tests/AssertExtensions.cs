using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AspNetCore.Examples.OpenTelemetry.TelemetryServices.Tests;

internal static class AssertExtensions
{
    extension(Assert)
    {
        public static void HasOptions(TelemetryOptions expected, ActivitySource actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Version, actual.Version);
            Assert.Equal(expected.Tags, actual.Tags);
        }

        public static void HasOptions(TelemetryOptions expected, Meter actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Version, actual.Version);
            Assert.Equal(expected.Tags, actual.Tags);
        }
    }
}
