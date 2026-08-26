using System;
using BackendVsaOwin.Host.Authentication;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace BackendVsaOwin.Host.OpenApi;

/// <summary>
/// Declares the Basic and OAuth2 schemes accepted by protected API operations.
/// </summary>
internal sealed class AuthenticationSecurityDocumentProcessor : IDocumentProcessor
{
    public void Process(DocumentProcessorContext context)
    {
        context.Document.SecurityDefinitions["basic"] =
            new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.Http,
                Scheme = "basic",
                Description = "HTTP Basic authentication.",
            };

        context.Document.SecurityDefinitions["oauth2"] =
            new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.OAuth2,
                Flow = OpenApiOAuth2Flow.Password,
                Flows = new OpenApiOAuthFlows
                {
                    Password = new OpenApiOAuthFlow
                    {
                        TokenUrl = OAuthEndpoints.TokenPath,
                        // Scope authorization is intentionally not implemented in this starter.
                        Scopes = new System.Collections.Generic.Dictionary<string, string>(),
                    },
                },
                Description = "OAuth2 password grant.",
            };

        // Separate requirement objects mean Basic OR OAuth2, not both at once.
        context.Document.Security = new[]
        {
            new OpenApiSecurityRequirement
            {
                ["basic"] = Array.Empty<string>(),
            },
            new OpenApiSecurityRequirement
            {
                ["oauth2"] = Array.Empty<string>(),
            },
        };
    }
}
