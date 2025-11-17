namespace ConfigMgrWebService.Shared.Responses;

/// <summary>
/// Standard API response wrapper for all endpoints
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The response data (null if operation failed)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// User-friendly message about the operation
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// List of errors (empty if successful)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Correlation ID for request tracking
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Timestamp of the response (ISO 8601)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully"
        };
    }

    /// <summary>
    /// Creates a successful response without data
    /// </summary>
    public static ApiResponse<T> SuccessResponse(string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message ?? "Operation completed successfully"
        };
    }

    /// <summary>
    /// Creates a failure response with a single error
    /// </summary>
    public static ApiResponse<T> FailureResponse(string error, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message ?? "Operation failed",
            Errors = new List<string> { error }
        };
    }

    /// <summary>
    /// Creates a failure response with multiple errors
    /// </summary>
    public static ApiResponse<T> FailureResponse(List<string> errors, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message ?? "Operation failed",
            Errors = errors
        };
    }
}

/// <summary>
/// Non-generic API response for operations without data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public new static ApiResponse SuccessResponse(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message ?? "Operation completed successfully"
        };
    }

    public new static ApiResponse FailureResponse(string error, string? message = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message ?? "Operation failed",
            Errors = new List<string> { error }
        };
    }

    public new static ApiResponse FailureResponse(List<string> errors, string? message = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message ?? "Operation failed",
            Errors = errors
        };
    }
}
