using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Support;
using Microsoft.Owin.Testing;
using Newtonsoft.Json.Linq;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.OpenApi;

public sealed class OpenApiEndpointTests
{
    [Fact]
    public async Task Get_describes_customer_and_order_endpoints()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        using var response = await client.GetAsync(
            "swagger/v1/swagger.json",
            TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"openapi\"", body);
        var document = JObject.Parse(body);
        Assert.Equal("/", (string)document["servers"]![0]!["url"]!);
        Assert.Contains("\"/api/customers\"", body);
        Assert.Contains("\"/api/customers/{id}\"", body);
        Assert.Contains("\"/api/orders\"", body);
        Assert.Contains("\"/api/orders/{id}\"", body);
        Assert.Contains("\"/api/orders/batch\"", body);
        Assert.Contains("\"/api/orders/batch-delete\"", body);
        Assert.Contains("/oauth/token", body);
        Assert.Contains("\"Authentication\"", body);
        Assert.Contains("application/x-www-form-urlencoded", body);
        Assert.Contains("grant_type", body);
        Assert.Contains("access_token", body);
        Assert.Contains("token_type", body);
        Assert.Contains("expires_in", body);
        Assert.Contains("error_description", body);
        Assert.Contains("\"securitySchemes\"", body);
        Assert.Contains("\"scheme\": \"basic\"", body);
        Assert.Contains("\"oauth2\"", body);
        Assert.Contains("\"flows\"", body);
        Assert.Contains("\"password\"", body);
        Assert.Contains("\"tokenUrl\": \"/oauth/token\"", body);
        Assert.Contains("\"security\"", body);
        Assert.Contains("\"500\": {", body);
        Assert.Contains("\"Internal server error.\"", body);
        Assert.Contains("\"application/problem+json\"", body);
        Assert.Contains("\"ProblemDetailsResponse\"", body);

        var tokenPath = JObject.Parse(body)["paths"]!["/oauth/token"]!["post"]!;
        Assert.Empty((JObject)tokenPath["security"]![0]!);
        Assert.Contains(
            "application/json",
            tokenPath["responses"]!["400"]!["content"]!.ToString());
        Assert.DoesNotContain(
            "application/problem+json",
            tokenPath["responses"]!["400"]!["content"]!.ToString());

        foreach (var path in (JObject)document["paths"]!)
        {
            if (path.Key == "/oauth/token")
            {
                continue;
            }

            foreach (var operation in (JObject)path.Value!)
            {
                if (operation.Key == "parameters")
                {
                    continue;
                }

                var responses = (JObject)operation.Value!["responses"]!;
                Assert.NotNull(responses["500"]);
            }
        }

        Assert.DoesNotContain("__integration-tests/fault", body);
    }

    [Fact]
    public async Task Swagger_ui_uses_the_configured_application_title()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        using var response = await client.GetAsync(
            "swagger/index.html",
            TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Backend VSA OWIN API", body);
    }

    [Fact]
    public async Task Get_uses_camel_case_for_json_schema_properties()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);

        using var response = await client.GetAsync(
            "swagger/v1/swagger.json",
            TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"displayName\"", body);
        Assert.Contains("\"customerId\"", body);
        Assert.Contains("\"totalAmount\"", body);
        Assert.Contains("\"createdCount\"", body);
        Assert.DoesNotContain("\"DisplayName\"", body);
        Assert.DoesNotContain("\"CustomerId\"", body);
        Assert.DoesNotContain("\"TotalAmount\"", body);
        Assert.DoesNotContain("\"CreatedCount\"", body);
    }

    [Fact]
    public async Task Get_includes_xml_documentation_descriptions()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);

        using var response = await client.GetAsync(
            "swagger/v1/swagger.json",
            TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(
            "Order management endpoints for creating, querying, updating, and deleting orders.",
            body);
        Assert.Contains(
            "Customer management endpoints for creating and querying customers.",
            body);
        Assert.Contains("Creates an order after validating the request.", body);
        Assert.Contains(
            "Existing customer identifier; the order captures the customer's current display name.",
            body);
        Assert.Contains("Total order amount; must be greater than zero.", body);
        Assert.Contains(
            "Creates between 1 and 100 orders after validating every item.",
            body);
        Assert.Contains(
            "Unique, non-empty order identifiers; must contain between 1 and 100 values.",
            body);
        Assert.Contains(
            "Identifiers that did not match an existing order.",
            body);
        Assert.Contains("Validation messages keyed by JSON field path.", body);
        Assert.Contains("\"BatchDeleteOrdersResponse\"", body);
        Assert.Contains("\"ProblemDetailsResponse\"", body);
        Assert.Contains("\"ValidationProblemDetailsResponse\"", body);
        Assert.Contains("\"type\"", body);
        Assert.Contains("\"title\"", body);
        Assert.Contains("\"status\"", body);
        Assert.Contains("\"detail\"", body);
        Assert.Contains("\"instance\"", body);
        Assert.Contains("\"traceId\"", body);
        Assert.Contains("\"application/problem+json\"", body);
        Assert.DoesNotContain("\"code\"", body);
    }
}
