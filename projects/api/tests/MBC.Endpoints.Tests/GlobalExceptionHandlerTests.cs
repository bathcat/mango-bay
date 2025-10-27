using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MBC.Endpoints.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MBC.Endpoints.Tests;

public class DevExceptionHandlerTests
{
    private static DevExceptionHandler CreateHandler()
    {
        return new DevExceptionHandler(NullLogger<DevExceptionHandler>.Instance);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task TryHandleAsync_UnauthorizedAccessException_Returns403ProblemDetails()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new UnauthorizedAccessException("Access denied");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(403, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);

        Assert.NotNull(problemDetails);
        Assert.Equal(403, problemDetails.Status);
        Assert.Equal("Forbidden", problemDetails.Title);
        Assert.Equal("Access denied", problemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_InvalidOperationException_Returns400ProblemDetails()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Invalid operation occurred");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(400, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);

        Assert.NotNull(problemDetails);
        Assert.Equal(400, problemDetails.Status);
        Assert.Equal("Bad Request", problemDetails.Title);
        Assert.Equal("Invalid operation occurred", problemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_ArgumentException_Returns400ProblemDetails()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new ArgumentException("Invalid argument provided");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(400, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);

        Assert.NotNull(problemDetails);
        Assert.Equal(400, problemDetails.Status);
        Assert.Equal("Invalid Argument", problemDetails.Title);
        Assert.Equal("Invalid argument provided", problemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_UnknownException_Returns500ProblemDetails()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new Exception("Something went wrong");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(500, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);

        Assert.NotNull(problemDetails);
        Assert.Equal(500, problemDetails.Status);
        Assert.Equal("Internal Server Error", problemDetails.Title);
        Assert.Equal("Something went wrong", problemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_NotImplementedException_Returns501ProblemDetails()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new NotImplementedException("Feature not yet implemented");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(501, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);

        Assert.NotNull(problemDetails);
        Assert.Equal(501, problemDetails.Status);
        Assert.Equal("Not Implemented", problemDetails.Title);
        Assert.Equal("Feature not yet implemented", problemDetails.Detail);
    }
}

public class ProdExceptionHandlerTests
{
    private static ProdExceptionHandler CreateHandler()
    {
        return new ProdExceptionHandler(NullLogger<ProdExceptionHandler>.Instance);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task TryHandleAsync_AnyException_Returns500WithGenericMessage()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new Exception("Sensitive error details");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(500, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);

        Assert.NotNull(problemDetails);
        Assert.Equal(500, problemDetails.Status);
        Assert.Equal("Internal Server Error", problemDetails.Title);
        Assert.Equal("An error occurred", problemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_InvalidOperationException_Returns500WithGenericMessage()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Sensitive operation details");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(500, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);

        Assert.NotNull(problemDetails);
        Assert.Equal(500, problemDetails.Status);
        Assert.Equal("Internal Server Error", problemDetails.Title);
        Assert.Equal("An error occurred", problemDetails.Detail);
    }
}

