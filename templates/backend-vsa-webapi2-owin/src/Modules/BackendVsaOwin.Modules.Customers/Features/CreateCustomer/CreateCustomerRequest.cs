namespace BackendVsaOwin.Modules.Customers.Features.CreateCustomer;

public sealed class CreateCustomerRequest
{
    /// <summary>
    /// Customer display name; required and limited to 100 characters.
    /// </summary>
    public string? DisplayName { get; set; }
}
