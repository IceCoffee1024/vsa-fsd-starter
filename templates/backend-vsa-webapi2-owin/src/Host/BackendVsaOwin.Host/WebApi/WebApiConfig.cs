using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using BackendVsaOwin.Host.Authentication;
using BackendVsaOwin.BuildingBlocks.WebApi.Tracing;
using BackendVsaOwin.BuildingBlocks.WebApi.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

namespace BackendVsaOwin.Host.WebApi;

internal static class WebApiConfig
{
    public static HttpConfiguration Create(
        IServiceProvider serviceProvider,
        IEnumerable<Assembly> controllerAssemblies)
    {
        var configuration = new HttpConfiguration
        {
            DependencyResolver = new MsDiDependencyResolver(serviceProvider),
            IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never,
        };

        configuration.Services.Replace(
            typeof(IAssembliesResolver),
            new ModuleAssembliesResolver(controllerAssemblies));
        configuration.MapHttpAttributeRoutes();
        configuration.MessageHandlers.Add(new RequestTraceHandler());
        configuration.Filters.Add(new AuthorizeAttribute());
        configuration.Filters.Add(new SchemeAwareAuthenticationFilter());
        configuration.Filters.Add(new ModelStateValidationFilter());
        configuration.Formatters.Remove(configuration.Formatters.XmlFormatter);
        configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
            new CamelCasePropertyNamesContractResolver();
        configuration.Services.Replace(
            typeof(IExceptionHandler),
            new ProblemDetailsExceptionHandler(
                serviceProvider.GetRequiredService<
                    ILogger<ProblemDetailsExceptionHandler>>()));

        return configuration;
    }
}
