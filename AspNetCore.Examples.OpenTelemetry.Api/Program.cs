using AspNetCore.Examples.OpenTelemetry.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
    .AddCustomOpenTelemetry(builder.Configuration);

builder.Services
    .AddCustomOpenApi()
    .AddCustomOpenTelemetry(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapCustomOpenApi();
    app.UseCustomOpenApiUI();
}

app.MapTelemetryEndpoints();

app.Run();
