using System.Collections.Generic;
using System.Reflection;
using BackendVsaOwin.Host.Authentication;
using BackendVsaOwin.Host.Composition;
using NJsonSchema;
using Newtonsoft.Json;
using NJsonSchema.NewtonsoftJson.Generation;
using NSwag;
using NSwag.AspNet.Owin;
using Owin;

namespace BackendVsaOwin.Host.OpenApi;

internal static class OpenApiAppBuilderExtensions
{
    public static IAppBuilder UseConfiguredOpenApi(
        this IAppBuilder app,
        IEnumerable<Assembly> controllerAssemblies,
        JsonSerializerSettings serializerSettings)
    {
        app.UseSwaggerUi(
            controllerAssemblies,
            settings =>
            {
                settings.Path = "/swagger";
                settings.DocumentTitle = ApplicationIdentity.OpenApiTitle;
                settings.OAuth2Client = new OAuth2ClientSettings
                {
                    AppName = ApplicationIdentity.ApplicationName,
                    ClientId = "swagger-client",
                };
                settings.PostProcess = PostProcessDocument;
                settings.GeneratorSettings.UseControllerSummaryAsTagDescription = true;
                settings.GeneratorSettings.DocumentProcessors.Add(
                    new AuthenticationSecurityDocumentProcessor());
                settings.GeneratorSettings.DocumentProcessors.Add(
                    new OAuthTokenDocumentProcessor());
                settings.GeneratorSettings.OperationProcessors.Add(
                    new ProblemDetailsResponseOperationProcessor());
                settings.GeneratorSettings.SchemaSettings =
                    new NewtonsoftJsonSchemaGeneratorSettings
                    {
                        SchemaType = SchemaType.OpenApi3,
                        SerializerSettings = serializerSettings,
                    };
            });

        return app;
    }

    private static void PostProcessDocument(OpenApiDocument document)
    {
        document.Servers.Clear();
        document.Servers.Add(
            new OpenApiServer
            {
                Url = "/",
            });

        UseProblemDetailsMediaType(document);
    }

    private static void UseProblemDetailsMediaType(OpenApiDocument document)
    {
        foreach (var operation in document.Operations)
        {
            if (string.Equals(
                    operation.Path,
                    OAuthEndpoints.TokenPath,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var responseEntry in operation.Operation.Responses)
            {
                if (!int.TryParse(responseEntry.Key, out var statusCode)
                    || statusCode < 400
                    || !responseEntry.Value.Content.TryGetValue(
                        "application/json",
                        out var mediaType))
                {
                    continue;
                }

                responseEntry.Value.Content.Remove("application/json");
                responseEntry.Value.Content["application/problem+json"] =
                    mediaType;
            }
        }
    }
}
