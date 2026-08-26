using System.Reflection;
using BackendVsaOwin.Modules.Customers.Contracts;
using BackendVsaOwin.Modules.Orders.Features;
using BackendVsaOwin.Modules.Orders.Features.BatchCreateOrders;
using BackendVsaOwin.Modules.Orders.Features.BatchDeleteOrders;
using BackendVsaOwin.Modules.Orders.Features.CreateOrder;
using BackendVsaOwin.Modules.Orders.Features.DeleteOrder;
using BackendVsaOwin.Modules.Orders.Features.GetOrder;
using BackendVsaOwin.Modules.Orders.Features.ListOrders;
using BackendVsaOwin.Modules.Orders.Features.UpdateOrder;
using BackendVsaOwin.Modules.Orders.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BackendVsaOwin.Modules.Orders;

public static class OrdersModule
{
    public static Assembly Assembly => typeof(OrdersModule).Assembly;

    public static IServiceCollection AddOrdersModule(this IServiceCollection services)
    {
        services.AddSingleton<IOrderStore, SqliteOrderStore>();
        services.AddTransient<BatchCreateOrdersValidator>();
        services.AddTransient(
            serviceProvider => new BatchCreateOrdersHandler(
                serviceProvider.GetRequiredService<IOrderStore>(),
                serviceProvider.GetRequiredService<ICustomerLookup>()));
        services.AddTransient<BatchDeleteOrdersValidator>();
        services.AddTransient(
            serviceProvider => new BatchDeleteOrdersHandler(
                serviceProvider.GetRequiredService<IOrderStore>()));
        services.AddTransient<CreateOrderValidator>();
        services.AddTransient(
            serviceProvider => new CreateOrderHandler(
                serviceProvider.GetRequiredService<IOrderStore>(),
                serviceProvider.GetRequiredService<ICustomerLookup>()));

        services.AddTransient(
            serviceProvider => new GetOrderHandler(
                serviceProvider.GetRequiredService<IOrderStore>()));

        services.AddTransient(
            serviceProvider => new ListOrdersHandler(
                serviceProvider.GetRequiredService<IOrderStore>()));

        services.AddTransient<UpdateOrderValidator>();
        services.AddTransient(
            serviceProvider => new UpdateOrderHandler(
                serviceProvider.GetRequiredService<IOrderStore>()));

        services.AddTransient(
            serviceProvider => new DeleteOrderHandler(
                serviceProvider.GetRequiredService<IOrderStore>()));
        services.AddTransient<OrdersController>();

        return services;
    }
}
