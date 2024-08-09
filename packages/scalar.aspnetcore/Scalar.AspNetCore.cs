using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Scalar.AspNetCore
{
    public static class OpenApiEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapScalarApiReference(this IEndpointRouteBuilder endpoints)
        {
              return endpoints.MapScalarApiReference(_ => { });
        }

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static IEndpointConventionBuilder MapScalarApiReference(this IEndpointRouteBuilder endpoints,
            Action<ScalarOptions> configureOptions)
        {
            var options = new ScalarOptions();
            configureOptions(options);
            var configurationJson = JsonSerializer.Serialize(options, JsonSerializerOptions);
            var configurationJsonBytes = Encoding.UTF8.GetBytes(configurationJson);
            var configurationJsonBytesBase64 = Convert.ToBase64String(configurationJsonBytes);
            return endpoints.MapGet(options.EndpointPathPrefix + "/{documentName}", (string documentName) =>
            {
                var title = options.Title ?? "Scalar API Reference -- " + documentName;
                var openApiSpecPath = options.OpenApiSpecPath.Replace("{documentName}", documentName);
                var contentHtml = $$"""
                                    <!doctype html>
                                    <html>
                                        <head>
                                            <title>{{title}}</title>
                                            <meta charset="utf-8" />
                                            <meta name="viewport" content="width=device-width, initial-scale=1" />
                                        </head>
                                        <body>
                                            <script id="api-reference" data-url="{{openApiSpecPath}}"></script>
                                            <script>
                                                document.getElementById('api-reference').dataset.configuration = atob('{{configurationJsonBytesBase64}}')
                                            </script>
                                            <script src="https://cdn.jsdelivr.net/npm/@scalar/api-reference"></script>
                                        </body>
                                    </html>
                                    """;
                return TypedResults.Content(contentHtml, "text/html");
            }).ExcludeFromDescription();
        }
    }
}
