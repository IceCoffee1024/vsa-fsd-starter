using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Customers;
using BackendVsaOwin.Host.IntegrationTests.Support;
using BackendVsaOwin.Modules.Orders.Features.CreateOrder;
using Microsoft.Owin.Testing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Orders;

internal static class OrderEndpointTestHelper
{
    public static async Task<CreateOrderResponse> CreateOrderAsync(
        TestServer server,
        string customerName,
        decimal totalAmount)
    {
        var customer = await CustomerEndpointTestHelper.CreateCustomerAsync(
            server,
            customerName);
        var request = new CreateOrderRequest
        {
            CustomerId = customer.Id,
            TotalAmount = totalAmount,
        };

        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        using var response = await client.PostAsJsonAsync("api/orders", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await response.Content.ReadAsAsync<CreateOrderResponse>(
            TestContext.Current.CancellationToken);
    }
}
