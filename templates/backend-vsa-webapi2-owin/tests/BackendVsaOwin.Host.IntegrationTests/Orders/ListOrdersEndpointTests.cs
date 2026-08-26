using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Support;
using BackendVsaOwin.Modules.Orders.Features.ListOrders;
using Microsoft.Owin.Testing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Orders;

public sealed class ListOrdersEndpointTests
{
    [Fact]
    public async Task Get_returns_created_orders()
    {
        using var server = TestServer.Create<Startup>();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var created = await OrderEndpointTestHelper.CreateOrderAsync(
            server,
            "Ada Lovelace",
            42.50m);

        using var response = await client.GetAsync(
            "api/orders",
            TestContext.Current.CancellationToken);
        var orders = await response.Content.ReadAsAsync<ListOrdersResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Id, Assert.Single(orders.Items).Id);
    }
}
