# AspNetCore.OpenTelemetry.Example

This project serves as an example of how to monitor an ASP.Net Core application via OpenTelemetry.

It makes use of the following technologies and projects:

- [.NET 7.0](https://dotnet.microsoft.com/es-es/download/dotnet/7.0)
- [ASP.NET Core 7.0](https://learn.microsoft.com/es-es/aspnet/core/?view=aspnetcore-7.0)
- [Swashbuckle](https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-7.0&tabs=visual-studio)
- [Docker](https://docs.docker.com/)
- [Docker compose](https://docs.docker.com/compose/)
- [OpenTelemetry](https://opentelemetry.io/)
- [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/) to receive the OpenTelemetry data from the application and distribute it to the proper backends
- [Grafana Tempo](https://grafana.com/oss/tempo/) as the tracing backend
- [Prometheus](https://prometheus.io/) as the metrics backend
- [Grafana Loki](https://grafana.com/oss/loki/) as the logs backend
- [Grafana](https://grafana.com/) as the observability UI

# How to run
Simply run the docker compose project from visual studio, or from a terminal by executing `docker compose up` in the root folder.

Access the web api at http://localhost:8080/swagger and try to create traces, metrics and logs by calling the corresponding endpoints.

Access grafana at http://localhost:3001 and check that the [traces](http://localhost:3001/explore?orgId=1&left=%7B%22datasource%22:%22tempo%22,%22queries%22:%5B%7B%22refId%22:%22A%22,%22datasource%22:%7B%22type%22:%22tempo%22,%22uid%22:%22tempo%22%7D,%22queryType%22:%22nativeSearch%22,%22limit%22:20,%22serviceName%22:%22AspNetCore.OpenTelemetry.Example.Api%22%7D%5D,%22range%22:%7B%22from%22:%22now-1h%22,%22to%22:%22now%22%7D%7D), [metrics](http://localhost:3001/explore?orgId=1&left=%7B%22datasource%22:%22prometheus%22,%22queries%22:%5B%7B%22refId%22:%22A%22,%22expr%22:%22%7Bexported_job%3D%5C%22AspNetCore.OpenTelemetry.Example.Api%5C%22%7D%22,%22range%22:true,%22instant%22:true,%22datasource%22:%7B%22type%22:%22prometheus%22,%22uid%22:%22prometheus%22%7D,%22editorMode%22:%22builder%22%7D%5D,%22range%22:%7B%22from%22:%22now-1h%22,%22to%22:%22now%22%7D%7D) and [logs](http://localhost:3001/explore?orgId=1&left=%7B%22datasource%22:%22loki%22,%22queries%22:%5B%7B%22refId%22:%22A%22,%22expr%22:%22%7Bjob%3D%5C%22AspNetCore.OpenTelemetry.Example.Api%5C%22%7D%20%7C%3D%20%60%60%22,%22queryType%22:%22range%22,%22datasource%22:%7B%22type%22:%22loki%22,%22uid%22:%22loki%22%7D,%22editorMode%22:%22builder%22%7D%5D,%22range%22:%7B%22from%22:%22now-1h%22,%22to%22:%22now%22%7D%7D) have been registered.