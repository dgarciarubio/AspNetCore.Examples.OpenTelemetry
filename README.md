# AspNetCore.Examples.OpenTelemetry

This project serves as an example of how to monitor an ASP.Net Core application via OpenTelemetry.

It makes use of the following technologies and projects:

- [.NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
- [ASP.NET Core 9.0](https://learn.microsoft.com/aspnet/core/?view=aspnetcore-9.0)
- [ScalaR](https://github.com/ScalaR/ScalaR)
- [OpenTelemetry](https://opentelemetry.io/)
- [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview)

# How to run
Simply run the AspireHost project from visual studio, or from a terminal by executing `dotnet run` in the Aspire project folder.

Access the web api [Scalar UI](http://localhost:5250/scalar/v1)
Call the weather forecast endpoint to register some telemetry data.

Access Aspire dashboard at http://localhost:15154, and check that the [traces](http://localhost:15154/traces), [metrics](http://localhost:15154/metrics) and [logs](http://localhost:15154/structuredlogs) have been registered.
