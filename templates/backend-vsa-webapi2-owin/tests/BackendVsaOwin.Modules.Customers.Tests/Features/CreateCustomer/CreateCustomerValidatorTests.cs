using BackendVsaOwin.Modules.Customers.Features.CreateCustomer;
using Xunit;

namespace BackendVsaOwin.Modules.Customers.Tests.Features.CreateCustomer;

public sealed class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _validator = new();

    [Fact]
    public void Validate_rejects_an_empty_display_name()
    {
        var errors = _validator.Validate(
            new CreateCustomerRequest
            {
                DisplayName = " ",
            });

        Assert.Contains("displayName", errors.Keys);
    }

    [Fact]
    public void Validate_accepts_a_valid_request()
    {
        var errors = _validator.Validate(
            new CreateCustomerRequest
            {
                DisplayName = "Ada Lovelace",
            });

        Assert.Empty(errors);
    }
}
