using AspNetCore.Examples.OpenTelemetry.Api.Extensions;
using AspNetCore.Examples.OpenTelemetry.Api.SampleTelemetry;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
    
builder.Services.AddCustomOpenApi();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(cors => cors.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    }));
}

builder.Services.AddSampleTelemetry();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors();
    app.MapCustomOpenApi();
    app.UseCustomOpenApiUI();
}

app.MapSampleTelemetryEndpoints();

app.Run();
