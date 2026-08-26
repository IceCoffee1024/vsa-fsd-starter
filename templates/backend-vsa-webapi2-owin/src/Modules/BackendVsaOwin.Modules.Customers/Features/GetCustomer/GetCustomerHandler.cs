using System;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Customers.Infrastructure;

namespace BackendVsaOwin.Modules.Customers.Features.GetCustomer;

public sealed class GetCustomerHandler
{
    private readonly ICustomerStore _customers;

    internal GetCustomerHandler(ICustomerStore customers)
    {
        _customers = customers;
    }

    public async Task<GetCustomerResponse?> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var customer = await _customers.GetAsync(id, cancellationToken);

        return customer is null
            ? null
            : new GetCustomerResponse
            {
                Id = customer.Id,
                DisplayName = customer.DisplayName,
            };
    }
}
