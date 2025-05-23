using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<AspNetCore_Examples_OpenTelemetry_Api>(nameof(AspNetCore_Examples_OpenTelemetry_Api).Replace("_", "-"))
    .WithHttpHealthCheck("/health");

builder.Build().Run();
