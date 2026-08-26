using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using BackendVsaOwin.Host.WebApi;
using BackendVsaOwin.BuildingBlocks.WebApi.Tracing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Pipeline;

public sealed class ProblemDetailsExceptionHandlerTests
{
    [Fact]
    public void Handle_logs_the_exception_with_the_response_trace_id()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "http://localhost/api/orders");
        var traceId = RequestTraceContext.GetTraceId(request);
        var exception = new InvalidOperationException(
            "Sensitive diagnostic detail.");
        var exceptionContext = new ExceptionContext(
            exception,
            new ExceptionContextCatchBlock(
                "IntegrationTest",
                isTopLevel: true,
                callsHandler: true),
            request);
        var context = new ExceptionHandlerContext(exceptionContext);
        var logger = new RecordingLogger<ProblemDetailsExceptionHandler>();
        var handler = new ProblemDetailsExceptionHandler(logger);

        handler.Handle(context);

        Assert.Same(exception, logger.Exception);
        Assert.Equal(LogLevel.Error, logger.LogLevel);
        Assert.Equal(traceId, logger.Properties["TraceId"]);
        Assert.Equal("POST", logger.Properties["HttpMethod"]);
        Assert.Equal("/api/orders", logger.Properties["RequestPath"]);
        Assert.NotNull(context.Result);
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public Exception? Exception { get; private set; }

        public LogLevel LogLevel { get; private set; }

        public IReadOnlyDictionary<string, object?> Properties { get; private set; } =
            new Dictionary<string, object?>();

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            LogLevel = logLevel;
            Exception = exception;
            Properties = ((IEnumerable<KeyValuePair<string, object?>>)state!)
                .ToDictionary(item => item.Key, item => item.Value);
        }
    }
}
