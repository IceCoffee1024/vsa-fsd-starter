using System.Collections.Generic;

namespace BackendVsaOwin.Modules.Orders.Features.BatchCreateOrders;

public sealed class BatchCreateOrdersResult
{
    private BatchCreateOrdersResult(
        BatchCreateOrdersResponse? response,
        IReadOnlyDictionary<string, string[]> errors)
    {
        Response = response;
        Errors = errors;
    }

    public BatchCreateOrdersResponse? Response { get; }

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    internal static BatchCreateOrdersResult Success(
        BatchCreateOrdersResponse response) =>
        new(response, new Dictionary<string, string[]>());

    internal static BatchCreateOrdersResult Failure(
        IReadOnlyDictionary<string, string[]> errors) =>
        new(null, errors);
}
