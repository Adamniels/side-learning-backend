using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SideLearning.Application.Common.Exceptions;

namespace SideLearning.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleAsync(context, ex);
        }
    }

    private static Task HandleAsync(HttpContext context, Exception exception)
    {
        var (status, code, title, detail, errors) = MapException(exception);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{status}"
        };

        problem.Extensions["code"] = code;
        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        return context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }

    private static (int Status, string Code, string Title, string? Detail, Dictionary<string, string[]>? Errors) MapException(
        Exception exception)
    {
        return exception switch
        {
            AppValidationException ve => (
                ve.StatusCode,
                ve.Code,
                "Validation failed",
                ve.Message,
                new Dictionary<string, string[]>(ve.Errors)),

            AppException ae => (
                ae.StatusCode,
                ae.Code,
                GetTitle(ae.StatusCode),
                ae.Message,
                null),

            ValidationException fve => (
                StatusCodes.Status400BadRequest,
                "validation_failed",
                "Validation failed",
                "One or more validation failures occurred.",
                fve.Errors
                    .GroupBy(e => string.IsNullOrEmpty(e.PropertyName) ? "_" : e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),

            _ => (
                StatusCodes.Status500InternalServerError,
                "internal_error",
                "An unexpected error occurred",
                null,
                null)
        };
    }

    private static string GetTitle(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Bad request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status404NotFound => "Not found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status500InternalServerError => "Server error",
        _ => "Error"
    };
}
