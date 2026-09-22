using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

/// <summary>
/// Middleware for centralized exception handling and error response formatting.
/// Catches all unhandled exceptions, logs them with Serilog, and returns standardized error responses.
/// </summary>
/// <remarks>
/// This middleware replaces ValidationExceptionMiddleware with a more comprehensive solution that:
/// - Handles all exception types (not just ValidationException)
/// - Formats responses according to .doc/general-api.md specification
/// - Logs errors with structured logging (Serilog)
/// - Maps exceptions to appropriate HTTP status codes
/// </remarks>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of ErrorHandlingMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">Logger for structured error logging</param>
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to handle the HTTP request.
    /// Catches any exceptions thrown by downstream middleware and converts them to error responses.
    /// </summary>
    /// <param name="context">The HTTP context</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the exception with Serilog (structured logging)
            _logger.LogError(ex,
                "An error occurred while processing the request. Path: {Path}, Method: {Method}",
                context.Request.Path,
                context.Request.Method);

            // Handle the exception and return formatted error response
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles different exception types and formats appropriate error responses.
    /// Maps exceptions to HTTP status codes and error response format.
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="exception">The exception that was thrown</param>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Map exception to status code and error response
        var (statusCode, errorResponse) = exception switch
        {
            // 400 Bad Request - Validation errors
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Type = "ValidationError",
                    Error = "Invalid input data",
                    Detail = "One or more validation errors occurred",
                    Errors = validationEx.Errors.Select(e => new ValidationErrorDetail
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage
                    })
                }
            ),

            // 400 Bad Request - Invalid operation
            InvalidOperationException invalidOpEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Type = "InvalidOperation",
                    Error = "Operation not allowed",
                    Detail = invalidOpEx.Message
                }
            ),

            // 400 Bad Request - Argument errors
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Type = "InvalidArgument",
                    Error = "Invalid argument provided",
                    Detail = argEx.Message
                }
            ),

            // 404 Not Found
            KeyNotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                new ErrorResponse
                {
                    Type = "ResourceNotFound",
                    Error = "Resource not found",
                    Detail = notFoundEx.Message
                }
            ),

            // 401 Unauthorized
            UnauthorizedAccessException _ => (
                StatusCodes.Status401Unauthorized,
                new ErrorResponse
                {
                    Type = "AuthenticationError",
                    Error = "Authentication failed",
                    Detail = "Invalid authentication token or credentials"
                }
            ),

            // 500 Internal Server Error - Default for unexpected exceptions
            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Type = "InternalServerError",
                    Error = "An unexpected error occurred",
                    Detail = "Please contact support if the problem persists"
                }
            )
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        // Serialize with camelCase (matches API convention)
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(errorResponse, jsonOptions)
        );
    }
}
