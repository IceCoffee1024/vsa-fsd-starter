using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using BackendVsaOwin.BuildingBlocks.WebApi.Tracing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Support;

internal static class ProblemDetailsAssert
{
    public static async Task<TProblem> ReadAsync<TProblem>(
        HttpResponseMessage response,
        string expectedType)
        where TProblem : ProblemDetailsResponse
    {
        var body = await response.Content.ReadAsStringAsync();
        var formatter = new JsonMediaTypeFormatter();
        formatter.SupportedMediaTypes.Add(
            new MediaTypeHeaderValue("application/problem+json"));
        var problem = await response.Content.ReadAsAsync<TProblem>(
            new[] { formatter },
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(expectedType, problem.Type);
        Assert.Equal((int)response.StatusCode, problem.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Title));
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
        Assert.Equal(
            response.RequestMessage?.RequestUri?.PathAndQuery,
            problem.Instance);
        Assert.Equal(32, problem.TraceId.Length);
        Assert.All(problem.TraceId, character =>
            Assert.True(Uri.IsHexDigit(character)));
        Assert.Equal(
            problem.TraceId,
            Assert.Single(response.Headers.GetValues(
                RequestTraceHandler.TraceIdHeaderName)));
        Assert.DoesNotContain("\"code\"", body);

        return problem;
    }
}
