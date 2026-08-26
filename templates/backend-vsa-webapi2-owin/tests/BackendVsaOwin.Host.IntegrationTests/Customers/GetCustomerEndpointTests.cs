using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Support;
using BackendVsaOwin.Modules.Customers.Features.GetCustomer;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using Microsoft.Owin.Testing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Customers;

public sealed class GetCustomerEndpointTests
{
    [Fact]
    public async Task Get_returns_the_created_customer()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var created = await CustomerEndpointTestHelper.CreateCustomerAsync(
            server,
            "Ada Lovelace");

        using var response = await client.GetAsync(
            $"api/customers/{created.Id}",
            TestContext.Current.CancellationToken);
        var customer = await response.Content.ReadAsAsync<GetCustomerResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Id, customer.Id);
        Assert.Equal(created.DisplayName, customer.DisplayName);
    }

    [Fact]
    public async Task Get_returns_not_found_for_an_unknown_customer()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var id = Guid.Parse("64a649c8-a26c-48e1-895d-5e0459a3aba0");

        using var response = await client.GetAsync(
            $"api/customers/{id}",
            TestContext.Current.CancellationToken);
        var problem = await ProblemDetailsAssert.ReadAsync<ProblemDetailsResponse>(
            response,
            "urn:backend-vsa-owin:problem:customer-not-found");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(id.ToString(), problem.Detail);
    }
}
