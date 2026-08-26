using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Support;
using BackendVsaOwin.Modules.Customers.Features.CreateCustomer;
using Microsoft.Owin.Testing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Customers;

internal static class CustomerEndpointTestHelper
{
    public static async Task<CreateCustomerResponse> CreateCustomerAsync(
        TestServer server,
        string displayName)
    {
        var request = new CreateCustomerRequest { DisplayName = displayName };
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        using var response = await client.PostAsJsonAsync(
            "api/customers",
            request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await response.Content.ReadAsAsync<CreateCustomerResponse>(
            TestContext.Current.CancellationToken);
    }
}
