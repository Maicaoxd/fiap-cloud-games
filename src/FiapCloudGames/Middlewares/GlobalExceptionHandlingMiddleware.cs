using FCG.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Middlewares
{
    public sealed class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception while processing {Method} {Path}.",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, exception);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var problemDetails = CreateProblemDetails(context, exception);

            context.Response.StatusCode = problemDetails.Status!.Value;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }

        private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
        {
            return exception switch
            {
                BadHttpRequestException => CreateProblemDetails(
                    context,
                    StatusCodes.Status400BadRequest,
                    "Erro de validação.",
                    "O corpo da requisição é obrigatório."),

                ArgumentException => CreateProblemDetails(
                    context,
                    StatusCodes.Status400BadRequest,
                    "Erro de validação.",
                    exception.Message),

                EmailAlreadyRegisteredException => CreateProblemDetails(
                    context,
                    StatusCodes.Status409Conflict,
                    "Conflito.",
                    exception.Message),

                _ => CreateProblemDetails(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "Erro interno.",
                    "Ocorreu um erro inesperado.")
            };
        }

        private static ProblemDetails CreateProblemDetails(
            HttpContext context,
            int statusCode,
            string title,
            string detail)
        {
            return new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };
        }
    }
}
