using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace BackendVsaOwin.Host.WebApi;

internal sealed class MsDiDependencyResolver : IDependencyResolver
{
    private readonly IServiceProvider _serviceProvider;

    public MsDiDependencyResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object? GetService(Type serviceType)
    {
        return _serviceProvider.GetService(serviceType);
    }

    public IEnumerable<object> GetServices(Type serviceType)
    {
        return _serviceProvider
            .GetServices(serviceType)
            .OfType<object>();
    }

    public IDependencyScope BeginScope()
    {
        return new MsDiDependencyScope(_serviceProvider.CreateScope());
    }

    public void Dispose()
    {
        // The OWIN host owns and disposes the root service provider.
    }

    private sealed class MsDiDependencyScope : IDependencyScope
    {
        private readonly IServiceScope _scope;

        public MsDiDependencyScope(IServiceScope scope)
        {
            _scope = scope;
        }

        public object? GetService(Type serviceType)
        {
            return _scope.ServiceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _scope.ServiceProvider
                .GetServices(serviceType)
                .OfType<object>();
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
