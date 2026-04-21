using System.Text.Json;
using FCG.Api.Middlewares;
using FCG.Application.Common;
using FCG.Application.Common.Exceptions;
using FCG.Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FCG.Tests.Api.Middlewares;

public sealed class GlobalExceptionHandlingMiddlewareTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public async Task InvokeAsync_QuandoOcorrerArgumentException_DeveRetornarBadRequest()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var middleware = CreateMiddleware(_ =>
            throw new ArgumentException(DomainMessages.Email.InvalidFormat));

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Title.ShouldBe("Erro de validação.");
        problemDetails.Detail.ShouldBe(DomainMessages.Email.InvalidFormat);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task InvokeAsync_QuandoOcorrerBadHttpRequestException_DeveRetornarBadRequest()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var middleware = CreateMiddleware(_ =>
            throw new BadHttpRequestException("Required request body is missing."));

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Title.ShouldBe("Erro de validação.");
        problemDetails.Detail.ShouldBe("O corpo da requisição é obrigatório.");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task InvokeAsync_QuandoOcorrerEmailAlreadyRegisteredException_DeveRetornarConflict()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var middleware = CreateMiddleware(_ =>
            throw new EmailAlreadyRegisteredException());

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
        problemDetails.Status.ShouldBe(StatusCodes.Status409Conflict);
        problemDetails.Title.ShouldBe("Conflito.");
        problemDetails.Detail.ShouldBe(ApplicationMessages.User.EmailAlreadyRegistered);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            },
            Request =
            {
                Path = "/api/users"
            }
        };
    }

    private static GlobalExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next)
    {
        var logger = Substitute.For<ILogger<GlobalExceptionHandlingMiddleware>>();

        return new GlobalExceptionHandlingMiddleware(next, logger);
    }

    private static async Task<ProblemDetails> ReadProblemDetailsAsync(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;

        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            httpContext.Response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return problemDetails!;
    }
}
