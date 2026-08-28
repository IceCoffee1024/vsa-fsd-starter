using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using BackendVsaOwin.Host.Composition;

namespace BackendVsaOwin.Host.Persistence;

internal static class MigrationCatalog
{
    private static readonly IReadOnlyList<Assembly> OrderedAssemblies =
        CreateAssemblies();

    public static IReadOnlyList<Assembly> Assemblies => OrderedAssemblies;

    private static IReadOnlyList<Assembly> CreateAssemblies()
    {
        var moduleAssemblies = ModuleCatalog.MigrationAssemblies;
        var assemblies = new Assembly[moduleAssemblies.Count + 1];
        assemblies[0] = typeof(MigrationCatalog).Assembly;
        for (var index = 0; index < moduleAssemblies.Count; index++)
        {
            assemblies[index + 1] = moduleAssemblies[index];
        }

        return new ReadOnlyCollection<Assembly>(assemblies);
    }
}
