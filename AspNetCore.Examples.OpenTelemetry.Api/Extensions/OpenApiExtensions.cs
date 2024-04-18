using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace AspNetCore.Examples.OpenTelemetry.Api.Extensions;

public static class OpenApiExtensions
{
    private const string DocumentName = "v1";
    private const string Pattern = "/openapi/{documentName}.json";
    private const string Url = "/openapi/v1.json";

    public static IServiceCollection AddCustomOpenApi(this IServiceCollection services)
    {
        return services.AddOpenApi(DocumentName, options =>
        {
            options.AddSchemaTransformer<EnumAsStringSchemaTransformer>();
        });
    }

    public static IEndpointRouteBuilder MapCustomOpenApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOpenApi(Pattern);
        return endpoints;
    }

    public static IApplicationBuilder UseCustomOpenApiUI(this IApplicationBuilder app)
    {
        return app.UseSwaggerUI(options =>
        {
            var hostingEnv = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            options.ConfigObject.Urls = [
                new UrlDescriptor { Name = $"{hostingEnv.ApplicationName} v1", Url = Url },
            ];
        });
    }

    private class EnumAsStringSchemaTransformer : IOpenApiSchemaTransformer
    {
        public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
        {
            if (!context.JsonTypeInfo.Type.IsEnum)
                return Task.CompletedTask;

            var enumType = context.JsonTypeInfo.Type;

            schema.Type = "string";

            var defaultIntValue = (schema.Default as OpenApiInteger)?.Value;
            var defaultEnumValue = Enum.ToObject(enumType, defaultIntValue ?? 0);
            schema.Default = new OpenApiString(defaultEnumValue.ToString());

            schema.Enum = Enum.GetNames(enumType)
                .Select(name => (IOpenApiAny)new OpenApiString(name))
                .ToArray();

            return Task.CompletedTask;
        }
    }
}
