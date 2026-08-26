using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BackendVsaOwin.Host.Composition;

internal sealed class ApplicationModule
{
    public ApplicationModule(
        string name,
        Assembly assembly,
        Action<IServiceCollection> registerServices,
        params string[] dependencies)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A module name is required.", nameof(name));
        }

        Name = name;
        Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        RegisterServices = registerServices
            ?? throw new ArgumentNullException(nameof(registerServices));
        Dependencies = new ReadOnlyCollection<string>(
            dependencies ?? throw new ArgumentNullException(nameof(dependencies)));
    }

    public string Name { get; }

    public Assembly Assembly { get; }

    public Action<IServiceCollection> RegisterServices { get; }

    public IReadOnlyCollection<string> Dependencies { get; }
}
