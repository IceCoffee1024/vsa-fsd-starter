using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using BackendVsaOwin.Modules.Customers;
using BackendVsaOwin.Modules.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace BackendVsaOwin.Host.Composition;

/// <summary>
/// Defines the ordered modules composed into this Host.
/// </summary>
internal static class ModuleCatalog
{
    private static readonly IReadOnlyList<ApplicationModule> ModuleDefinitions =
        CreateModules();

    private static readonly IReadOnlyList<Assembly> Assemblies =
        CreateAssemblies(ModuleDefinitions);

    public static IReadOnlyList<ApplicationModule> Modules => ModuleDefinitions;

    public static IReadOnlyList<Assembly> ControllerAssemblies => Assemblies;

    public static IReadOnlyList<Assembly> MigrationAssemblies => Assemblies;

    public static void RegisterServices(IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        foreach (var module in Modules)
        {
            module.RegisterServices(services);
        }
    }

    private static IReadOnlyList<ApplicationModule> CreateModules()
    {
        var modules = new[]
        {
            new ApplicationModule(
                "Customers",
                CustomersModule.Assembly,
                services => services.AddCustomersModule()),
            new ApplicationModule(
                "Orders",
                OrdersModule.Assembly,
                services => services.AddOrdersModule(),
                "Customers"),
        };

        Validate(modules);
        return new ReadOnlyCollection<ApplicationModule>(modules);
    }

    private static IReadOnlyList<Assembly> CreateAssemblies(
        IReadOnlyList<ApplicationModule> modules)
    {
        var assemblies = new Assembly[modules.Count];
        for (var index = 0; index < modules.Count; index++)
        {
            assemblies[index] = modules[index].Assembly;
        }

        return new ReadOnlyCollection<Assembly>(assemblies);
    }

    private static void Validate(IReadOnlyList<ApplicationModule> modules)
    {
        var registeredNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var module in modules)
        {
            if (!registeredNames.Add(module.Name))
            {
                throw new InvalidOperationException(
                    $"Module '{module.Name}' is registered more than once.");
            }

            foreach (var dependency in module.Dependencies)
            {
                if (!registeredNames.Contains(dependency))
                {
                    throw new InvalidOperationException(
                        $"Module '{module.Name}' depends on '{dependency}', "
                        + "which must be registered earlier.");
                }
            }
        }
    }
}
