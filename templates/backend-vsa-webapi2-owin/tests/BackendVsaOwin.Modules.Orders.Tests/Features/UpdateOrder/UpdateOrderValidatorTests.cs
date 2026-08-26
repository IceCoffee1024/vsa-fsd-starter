using BackendVsaOwin.Modules.Orders.Features.UpdateOrder;
using Xunit;

namespace BackendVsaOwin.Modules.Orders.Tests.Features.UpdateOrder;

public sealed class UpdateOrderValidatorTests
{
    private readonly UpdateOrderValidator _validator = new();

    [Fact]
    public void Validate_rejects_a_non_positive_total()
    {
        var errors = _validator.Validate(
            new UpdateOrderRequest
            {
                TotalAmount = 0,
            });

        Assert.Contains("totalAmount", errors.Keys);
    }

    [Fact]
    public void Validate_accepts_a_valid_request()
    {
        var errors = _validator.Validate(
            new UpdateOrderRequest
            {
                TotalAmount = 99.95m,
            });

        Assert.Empty(errors);
    }
}
