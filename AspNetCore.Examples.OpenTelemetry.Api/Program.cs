using AspNetCore.Examples.OpenTelemetry.Api.WeatherForecast;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

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

builder.Services.AddWeatherForecast();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapWeatherForecast();

app.Run();
