namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Standard error response format according to API documentation.
/// Used by ErrorHandlingMiddleware to return consistent error responses.
/// </summary>
/// <remarks>
/// Format specification from .doc/general-api.md:
/// <code>
/// {
///   "type": "ValidationError",
///   "error": "Invalid input data",
///   "detail": "The quantity field must be between 1 and 20"
/// }
/// </code>
/// </remarks>
public class ErrorResponse
{
    /// <summary>
    /// Machine-readable error type identifier.
    /// Examples: ValidationError, ResourceNotFound, AuthenticationError, InternalServerError
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Short, human-readable summary of the problem.
    /// Example: "Invalid input data"
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable explanation specific to this occurrence of the problem.
    /// Example: "The quantity field must be between 1 and 20"
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// Optional: List of validation errors when multiple fields fail validation.
    /// Only populated for ValidationError type.
    /// </summary>
    public IEnumerable<ValidationErrorDetail>? Errors { get; set; }
}

/// <summary>
/// Detailed validation error for a specific field.
/// </summary>
public class ValidationErrorDetail
{
    /// <summary>
    /// The field or property name that failed validation.
    /// Example: "items[0].quantity"
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// The validation error message for this field.
    /// Example: "Quantity cannot exceed 20"
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
