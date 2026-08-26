using System;
using BackendVsaOwin.Modules.Orders.Features.CreateOrder;
using Xunit;

namespace BackendVsaOwin.Modules.Orders.Tests.Features.CreateOrder;

public sealed class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator = new();

    [Fact]
    public void Validate_rejects_an_empty_customer_id_and_non_positive_total()
    {
        var errors = _validator.Validate(
            new CreateOrderRequest
            {
                CustomerId = Guid.Empty,
                TotalAmount = 0,
            });

        Assert.Contains("customerId", errors.Keys);
        Assert.Contains("totalAmount", errors.Keys);
    }

    [Fact]
    public void Validate_accepts_a_valid_request()
    {
        var errors = _validator.Validate(
            new CreateOrderRequest
            {
                CustomerId = Guid.Parse("d7231233-8af5-4e3c-ad61-38a78015bfec"),
                TotalAmount = 42.50m,
            });

        Assert.Empty(errors);
    }
}
