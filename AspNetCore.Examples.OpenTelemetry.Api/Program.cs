using AspNetCore.Examples.OpenTelemetry.Api;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services
    .AddSampleTelemetryEndpoint()
    .AddClassicTelemetryEndpoint();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapDefaultEndpoints();

app.MapSampleTelemetryEndpoint();
app.MapClassicTelemetryEndpoint();

app.Run();



