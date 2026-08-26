using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Orders;
using BackendVsaOwin.Host.IntegrationTests.Support;
using BackendVsaOwin.Modules.Orders.Features.GetOrder;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Persistence;

public sealed class DatabasePersistenceTests
{
    [Fact]
    public async Task Data_survives_a_host_restart_using_the_same_database_file()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "BackendVsaOwin.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var databasePath = Path.Combine(directory, "restart.db");

        try
        {
            Guid orderId;
            using (var firstServer = TestServerFactory.Create(databasePath))
            {
                var created = await OrderEndpointTestHelper.CreateOrderAsync(
                    firstServer,
                    "Ada Lovelace",
                    42.50m);
                orderId = created.Id;
            }

            using var secondServer = TestServerFactory.Create(databasePath);
            using var client = TestServerFactory.CreateAuthenticatedClient(secondServer);
            using var response = await client.GetAsync(
                $"api/orders/{orderId}",
                TestContext.Current.CancellationToken);
            var order = await response.Content.ReadAsAsync<GetOrderResponse>(
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(orderId, order.Id);
            Assert.Equal("Ada Lovelace", order.CustomerName);
            Assert.Equal(42.50m, order.TotalAmount);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
