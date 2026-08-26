using System;
using System.Linq;
using BackendVsaOwin.Host.Authentication;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace BackendVsaOwin.Host.OpenApi;

/// <summary>
/// Documents the OAuth token endpoint, which is handled by OWIN rather than Web API.
/// </summary>
internal sealed class OAuthTokenDocumentProcessor : IDocumentProcessor
{
    public void Process(DocumentProcessorContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (!context.Document.Tags.Any(tag => tag.Name == "Authentication"))
        {
            context.Document.Tags.Add(
                new OpenApiTag
                {
                    Name = "Authentication",
                    Description = "Authentication and token endpoints.",
                });
        }

        var operation = new OpenApiOperation
        {
            OperationId = "OAuthToken",
            Summary = "Issue an OAuth2 bearer token.",
            Description =
                "Issues a demo bearer token using the password grant and configured resource-owner credentials.",
            // An empty security requirement object explicitly allows anonymous access.
            Security = new[]
            {
                new OpenApiSecurityRequirement(),
            },
        };
        operation.Tags.Add("Authentication");
        operation.RequestBody = CreateTokenRequestBody();
        operation.Responses["200"] = CreateResponse(
            "Token issued.",
            "application/json",
            CreateTokenResponseSchema());
        operation.Responses["400"] = CreateResponse(
            "OAuth token request rejected.",
            "application/json",
            CreateTokenErrorSchema());

        var pathItem = new OpenApiPathItem();
        pathItem[OpenApiOperationMethod.Post] = operation;
        context.Document.Paths[OAuthEndpoints.TokenPath] = pathItem;
    }

    private static OpenApiRequestBody CreateTokenRequestBody()
    {
        var schema = new JsonSchema
        {
            Type = JsonObjectType.Object,
        };
        var grantType = AddRequiredStringProperty(schema, "grant_type");
        grantType.Enumeration.Add("password");
        AddRequiredStringProperty(schema, "username");
        AddRequiredStringProperty(schema, "password");

        var requestBody = new OpenApiRequestBody
        {
            Description = "OAuth2 password-grant form fields.",
            IsRequired = true,
        };
        requestBody.Content["application/x-www-form-urlencoded"] =
            new OpenApiMediaType
            {
                Schema = schema,
            };
        return requestBody;
    }

    private static OpenApiResponse CreateResponse(
        string description,
        string mediaTypeName,
        JsonSchema schema)
    {
        var response = new OpenApiResponse
        {
            Description = description,
        };
        response.Content[mediaTypeName] = new OpenApiMediaType
        {
            Schema = schema,
        };
        return response;
    }

    private static JsonSchema CreateTokenResponseSchema()
    {
        var schema = new JsonSchema
        {
            Type = JsonObjectType.Object,
        };
        AddRequiredStringProperty(schema, OAuthParameters.AccessToken);
        AddRequiredStringProperty(schema, "token_type");
        schema.Properties["expires_in"] = new JsonSchemaProperty
        {
            Type = JsonObjectType.Integer,
        };
        return schema;
    }

    private static JsonSchema CreateTokenErrorSchema()
    {
        var schema = new JsonSchema
        {
            Type = JsonObjectType.Object,
        };
        AddRequiredStringProperty(schema, "error");
        schema.Properties["error_description"] = new JsonSchemaProperty
        {
            Type = JsonObjectType.String,
        };
        return schema;
    }

    private static JsonSchemaProperty AddRequiredStringProperty(
        JsonSchema schema,
        string name)
    {
        var property = new JsonSchemaProperty
        {
            Type = JsonObjectType.String,
            IsRequired = true,
        };
        schema.Properties[name] = property;
        return property;
    }
}
