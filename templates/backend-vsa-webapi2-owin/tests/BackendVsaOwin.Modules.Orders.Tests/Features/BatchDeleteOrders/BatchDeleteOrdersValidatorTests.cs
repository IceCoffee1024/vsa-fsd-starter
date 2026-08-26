using System;
using System.Linq;
using BackendVsaOwin.Modules.Orders.Features.BatchDeleteOrders;
using Xunit;

namespace BackendVsaOwin.Modules.Orders.Tests.Features.BatchDeleteOrders;

public sealed class BatchDeleteOrdersValidatorTests
{
    private readonly BatchDeleteOrdersValidator _validator = new();

    [Fact]
    public void Validate_requires_at_least_one_id()
    {
        var errors = _validator.Validate(
            new BatchDeleteOrdersRequest
            {
                Ids = Array.Empty<Guid>(),
            });

        Assert.Contains("ids", errors.Keys);
    }

    [Fact]
    public void Validate_rejects_empty_and_duplicate_ids()
    {
        var id = Guid.Parse("b3a52ba9-92f0-4ae1-8c65-c1822305a6d4");
        var errors = _validator.Validate(
            new BatchDeleteOrdersRequest
            {
                Ids = new[] { Guid.Empty, id, id },
            });

        Assert.Equal(2, errors["ids"].Length);
    }

    [Fact]
    public void Validate_rejects_more_than_100_ids()
    {
        var ids = Enumerable.Range(1, 101)
            .Select(value => new Guid(value, 0, 0, new byte[8]))
            .ToArray();
        var errors = _validator.Validate(
            new BatchDeleteOrdersRequest
            {
                Ids = ids,
            });

        Assert.Contains("ids", errors.Keys);
    }

    [Fact]
    public void Validate_accepts_up_to_100_unique_non_empty_ids()
    {
        var ids = Enumerable.Range(1, 100)
            .Select(value => new Guid(value, 0, 0, new byte[8]))
            .ToArray();
        var errors = _validator.Validate(
            new BatchDeleteOrdersRequest
            {
                Ids = ids,
            });

        Assert.Empty(errors);
    }
}
