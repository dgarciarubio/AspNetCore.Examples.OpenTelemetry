using AspNetCore.OpenTelemetry.Example.Api.Extensions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
    .AddCustomOpenTelemetry(builder.Configuration);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddCustomOpenTelemetry(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
