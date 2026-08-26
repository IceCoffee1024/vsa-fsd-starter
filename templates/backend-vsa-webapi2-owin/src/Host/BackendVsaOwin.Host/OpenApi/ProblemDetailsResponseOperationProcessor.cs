using System;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace BackendVsaOwin.Host.OpenApi;

/// <summary>
/// Adds the shared internal-server-error contract to every generated operation.
/// </summary>
internal sealed class ProblemDetailsResponseOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var responses = context.OperationDescription.Operation.Responses;
        if (responses.ContainsKey("500"))
        {
            return true;
        }

        var schema = context.SchemaGenerator.Generate(
            typeof(ProblemDetailsResponse),
            context.SchemaResolver);

        var response = new OpenApiResponse
        {
            Description = "Internal server error.",
        };
        response.Content["application/problem+json"] = new OpenApiMediaType
        {
            Schema = schema,
        };
        responses["500"] = response;

        return true;
    }
}
