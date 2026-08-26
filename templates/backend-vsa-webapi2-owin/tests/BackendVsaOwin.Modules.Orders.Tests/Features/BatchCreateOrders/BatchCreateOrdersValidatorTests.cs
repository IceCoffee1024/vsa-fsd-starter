using System;
using System.Linq;
using BackendVsaOwin.Modules.Orders.Features.BatchCreateOrders;
using Xunit;

namespace BackendVsaOwin.Modules.Orders.Tests.Features.BatchCreateOrders;

public sealed class BatchCreateOrdersValidatorTests
{
    private readonly BatchCreateOrdersValidator _validator = new();

    [Fact]
    public void Validate_requires_at_least_one_order()
    {
        var errors = _validator.Validate(
            new BatchCreateOrdersRequest
            {
                Orders = System.Array.Empty<BatchCreateOrderItem?>(),
            });

        Assert.Contains("orders", errors.Keys);
    }

    [Fact]
    public void Validate_rejects_more_than_100_orders()
    {
        var orders = Enumerable.Range(1, 101)
            .Select(_ => (BatchCreateOrderItem?)new BatchCreateOrderItem
            {
                CustomerId = Guid.NewGuid(),
                TotalAmount = 42.50m,
            })
            .ToArray();
        var errors = _validator.Validate(
            new BatchCreateOrdersRequest
            {
                Orders = orders,
            });

        Assert.Contains("orders", errors.Keys);
    }

    [Fact]
    public void Validate_reports_indexed_item_errors()
    {
        var errors = _validator.Validate(
            new BatchCreateOrdersRequest
            {
                Orders = new BatchCreateOrderItem?[]
                {
                    null,
                    new()
                    {
                        CustomerId = Guid.Empty,
                        TotalAmount = 0,
                    },
                },
            });

        Assert.Contains("orders[0]", errors.Keys);
        Assert.Contains("orders[1].customerId", errors.Keys);
        Assert.Contains("orders[1].totalAmount", errors.Keys);
    }

    [Fact]
    public void Validate_accepts_up_to_100_valid_orders()
    {
        var orders = Enumerable.Range(1, 100)
            .Select(_ => (BatchCreateOrderItem?)new BatchCreateOrderItem
            {
                CustomerId = Guid.NewGuid(),
                TotalAmount = 42.50m,
            })
            .ToArray();
        var errors = _validator.Validate(
            new BatchCreateOrdersRequest
            {
                Orders = orders,
            });

        Assert.Empty(errors);
    }
}
