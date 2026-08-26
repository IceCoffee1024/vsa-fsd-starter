using System;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Customers.Domain;
using BackendVsaOwin.Modules.Customers.Infrastructure;

namespace BackendVsaOwin.Modules.Customers.Features.CreateCustomer;

public sealed class CreateCustomerHandler
{
    private readonly Func<Guid> _createId;
    private readonly ICustomerStore _customers;

    internal CreateCustomerHandler(
        ICustomerStore customers,
        Func<Guid>? createId = null)
    {
        _customers = customers;
        _createId = createId ?? Guid.NewGuid;
    }

    public async Task<CreateCustomerResponse> HandleAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = new Customer(
            _createId(),
            request.DisplayName!.Trim());

        await _customers
            .AddAsync(customer, cancellationToken)
            .ConfigureAwait(false);

        return new CreateCustomerResponse
        {
            Id = customer.Id,
            DisplayName = customer.DisplayName,
        };
    }
}
