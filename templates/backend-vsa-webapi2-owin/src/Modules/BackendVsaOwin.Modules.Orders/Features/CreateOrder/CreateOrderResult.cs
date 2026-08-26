using System.Collections.Generic;

namespace BackendVsaOwin.Modules.Orders.Features.CreateOrder;

public sealed class CreateOrderResult
{
    private CreateOrderResult(
        CreateOrderResponse? response,
        IReadOnlyDictionary<string, string[]> errors)
    {
        Response = response;
        Errors = errors;
    }

    public CreateOrderResponse? Response { get; }

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    internal static CreateOrderResult Success(CreateOrderResponse response) =>
        new(response, new Dictionary<string, string[]>());

    internal static CreateOrderResult Failure(string field, string message) =>
        new(
            null,
            new Dictionary<string, string[]>
            {
                [field] = new[] { message },
            });
}
