using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Support;
using BackendVsaOwin.Modules.Customers.Features.CreateCustomer;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using Microsoft.Owin.Testing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Customers;

public sealed class CreateCustomerEndpointTests
{
    [Fact]
    public async Task Post_returns_the_created_customer()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        using var request = new StringContent(
            "{\"displayName\":\"  Ada Lovelace  \"}",
            Encoding.UTF8,
            "application/json");

        using var response = await client.PostAsync(
            "api/customers",
            request,
            TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync();
        var customer = await response.Content.ReadAsAsync<CreateCustomerResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/customers/{customer.Id}", response.Headers.Location?.AbsolutePath);
        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal("Ada Lovelace", customer.DisplayName);
        Assert.Contains("\"displayName\"", json);
        Assert.DoesNotContain("\"DisplayName\"", json);
    }

    [Fact]
    public async Task Post_rejects_an_empty_display_name()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var request = new CreateCustomerRequest { DisplayName = " " };

        using var response = await client.PostAsJsonAsync(
            "api/customers",
            request);
        var problem = await ProblemDetailsAssert
            .ReadAsync<ValidationProblemDetailsResponse>(
                response,
                "urn:backend-vsa-owin:problem:validation-failed");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("displayName", problem.Errors.Keys);
    }

    [Fact]
    public async Task Post_rejects_malformed_json_with_a_problem_response()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        using var request = new StringContent(
            "{",
            Encoding.UTF8,
            "application/json");

        using var response = await client.PostAsync(
            "api/customers",
            request,
            TestContext.Current.CancellationToken);
        var problem = await ProblemDetailsAssert
            .ReadAsync<ValidationProblemDetailsResponse>(
                response,
                "urn:backend-vsa-owin:problem:validation-failed");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("request", problem.Errors.Keys);
        Assert.Equal(
            "The request body is invalid.",
            Assert.Single(problem.Errors["request"]));
    }
}
