using System.Linq;
using BackendVsaOwin.Host.Composition;
using BackendVsaOwin.Host.Persistence;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Composition;

public sealed class ModuleCatalogTests
{
    [Fact]
    public void Catalog_drives_registration_discovery_and_migrations_in_dependency_order()
    {
        Assert.Collection(
            ModuleCatalog.Modules,
            customers =>
            {
                Assert.Equal("Customers", customers.Name);
                Assert.Empty(customers.Dependencies);
            },
            orders =>
            {
                Assert.Equal("Orders", orders.Name);
                Assert.Equal("Customers", Assert.Single(orders.Dependencies));
            });

        var moduleAssemblies = ModuleCatalog.Modules
            .Select(module => module.Assembly)
            .ToArray();
        Assert.Equal(moduleAssemblies, ModuleCatalog.ControllerAssemblies);
        Assert.Equal(moduleAssemblies, ModuleCatalog.MigrationAssemblies);
        Assert.Equal(
            typeof(MigrationCatalog).Assembly,
            MigrationCatalog.Assemblies[0]);
        Assert.Equal(
            moduleAssemblies,
            MigrationCatalog.Assemblies.Skip(1));
    }
}
