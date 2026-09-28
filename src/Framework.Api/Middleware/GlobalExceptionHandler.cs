using AutoMapper;
using Framework.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Framework.Api.Middleware;

internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception for request {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        ProblemDetails problemDetails = exception switch
        {
            DomainException domainException => CreateProblemDetails(httpContext, domainException.StatusCode, "Domain rule violation", domainException.Message),
            ArgumentException argumentException => CreateProblemDetails(httpContext, StatusCodes.Status400BadRequest, "Invalid request", argumentException.Message),
            KeyNotFoundException keyNotFoundException => CreateProblemDetails(httpContext, StatusCodes.Status404NotFound, "Resource not found", keyNotFoundException.Message),
            AutoMapperMappingException mappingException => CreateProblemDetails(httpContext, StatusCodes.Status500InternalServerError, "Mapping error", mappingException.Message),
            InvalidOperationException invalidOperationException => CreateProblemDetails(httpContext, StatusCodes.Status500InternalServerError, "Configuration error", invalidOperationException.Message),
            _ => CreateProblemDetails(httpContext, StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    private static ProblemDetails CreateProblemDetails(HttpContext httpContext, int statusCode, string title, string detail) =>
        new()
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
}

