using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace BackendVsaOwin.Host.WebApi;

/// <summary>
/// Restricts Web API controller discovery to the Host's declared module assemblies.
/// </summary>
internal sealed class ModuleAssembliesResolver : IAssembliesResolver
{
    private readonly Assembly[] _assemblies;

    public ModuleAssembliesResolver(IEnumerable<Assembly> assemblies)
    {
        if (assemblies is null)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        _assemblies = assemblies
            .Where(assembly => assembly is not null)
            .Distinct()
            .ToArray();
    }

    public ICollection<Assembly> GetAssemblies()
    {
        return _assemblies;
    }
}
