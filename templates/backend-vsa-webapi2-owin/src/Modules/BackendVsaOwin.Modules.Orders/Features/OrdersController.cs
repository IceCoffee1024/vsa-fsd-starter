using System;
using System.Net;
using System.Web.Http;
using BackendVsaOwin.Modules.Orders.Features.BatchCreateOrders;
using BackendVsaOwin.Modules.Orders.Features.BatchDeleteOrders;
using BackendVsaOwin.Modules.Orders.Features.CreateOrder;
using BackendVsaOwin.Modules.Orders.Features.DeleteOrder;
using BackendVsaOwin.Modules.Orders.Features.GetOrder;
using BackendVsaOwin.Modules.Orders.Features.ListOrders;
using BackendVsaOwin.Modules.Orders.Features.UpdateOrder;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;

namespace BackendVsaOwin.Modules.Orders.Features;

/// <summary>
/// Order management endpoints for creating, querying, updating, and deleting orders.
/// </summary>
[RoutePrefix("api/orders")]
public sealed partial class OrdersController : ApiController
{
    private const string OrderNotFoundProblemType =
        ProblemTypeUris.Prefix + ":order-not-found";

    private readonly BatchCreateOrdersHandler _batchCreateHandler;
    private readonly BatchCreateOrdersValidator _batchCreateValidator;
    private readonly BatchDeleteOrdersHandler _batchDeleteHandler;
    private readonly BatchDeleteOrdersValidator _batchDeleteValidator;
    private readonly CreateOrderHandler _createHandler;
    private readonly CreateOrderValidator _createValidator;
    private readonly DeleteOrderHandler _deleteHandler;
    private readonly GetOrderHandler _getHandler;
    private readonly ListOrdersHandler _listHandler;
    private readonly UpdateOrderHandler _updateHandler;
    private readonly UpdateOrderValidator _updateValidator;

    public OrdersController(
        BatchCreateOrdersValidator batchCreateValidator,
        BatchCreateOrdersHandler batchCreateHandler,
        BatchDeleteOrdersValidator batchDeleteValidator,
        BatchDeleteOrdersHandler batchDeleteHandler,
        CreateOrderValidator createValidator,
        CreateOrderHandler createHandler,
        GetOrderHandler getHandler,
        ListOrdersHandler listHandler,
        UpdateOrderValidator updateValidator,
        UpdateOrderHandler updateHandler,
        DeleteOrderHandler deleteHandler)
    {
        _batchCreateValidator = batchCreateValidator;
        _batchCreateHandler = batchCreateHandler;
        _batchDeleteValidator = batchDeleteValidator;
        _batchDeleteHandler = batchDeleteHandler;
        _createValidator = createValidator;
        _createHandler = createHandler;
        _getHandler = getHandler;
        _listHandler = listHandler;
        _updateValidator = updateValidator;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
    }

    private IHttpActionResult OrderNotFound(Guid id)
    {
        return this.Problem(
            HttpStatusCode.NotFound,
            OrderNotFoundProblemType,
            "Order not found",
            $"No order with identifier '{id}' exists.");
    }
}
