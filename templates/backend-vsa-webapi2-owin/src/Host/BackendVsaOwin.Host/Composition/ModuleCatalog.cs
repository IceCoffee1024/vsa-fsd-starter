using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using BackendVsaOwin.Modules.Customers;
using BackendVsaOwin.Modules.Orders;

namespace BackendVsaOwin.Host.Composition;

/// <summary>
/// Defines the module assemblies allowed to contribute Web API controllers.
/// </summary>
internal static class ModuleCatalog
{
    public static IReadOnlyCollection<Assembly> Assemblies { get; } =
        new ReadOnlyCollection<Assembly>(
            new[]
            {
                CustomersModule.Assembly,
                OrdersModule.Assembly,
            });
}
