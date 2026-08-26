using System.Reflection;
using BackendVsaOwin.Modules.Customers.Contracts;
using BackendVsaOwin.Modules.Customers.Features;
using BackendVsaOwin.Modules.Customers.Features.CreateCustomer;
using BackendVsaOwin.Modules.Customers.Features.GetCustomer;
using BackendVsaOwin.Modules.Customers.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BackendVsaOwin.Modules.Customers;

public static class CustomersModule
{
    public static Assembly Assembly => typeof(CustomersModule).Assembly;

    public static IServiceCollection AddCustomersModule(
        this IServiceCollection services)
    {
        services.AddSingleton<ICustomerStore, InMemoryCustomerStore>();
        services.AddSingleton<ICustomerLookup, CustomerLookup>();
        services.AddTransient<CreateCustomerValidator>();
        services.AddTransient(
            serviceProvider => new CreateCustomerHandler(
                serviceProvider.GetRequiredService<ICustomerStore>()));
        services.AddTransient(
            serviceProvider => new GetCustomerHandler(
                serviceProvider.GetRequiredService<ICustomerStore>()));
        services.AddTransient<CustomersController>();

        return services;
    }
}
