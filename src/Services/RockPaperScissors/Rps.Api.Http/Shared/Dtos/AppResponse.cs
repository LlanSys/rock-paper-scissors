using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace Rps.Api.Http.Shared.Dtos;

public record AppResponse<T>
{
    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public string? SessionId { get; init; }
    public T? Data { get; init; }

    // Failures are converted to ProblemDetails by BaseController.
    [JsonIgnore]
    public AppError? Error { get; }

    public AppResponse() { }

    public AppResponse(T? data) 
    { 
        IsSuccess = true;
        Data = data;
        Message = "Operation Successful";
    }

    [JsonConstructor]
    internal AppResponse(bool isSuccess, string? message, T? data)
    {
        IsSuccess = isSuccess;
        Message = message ?? "Operation Successful";
        Data = data;
    }

    internal AppResponse(AppError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        IsSuccess = false;
        Error = error;
    }

    public AppResponse(T? data, string message)
    {
        IsSuccess = true;
        Data = data;
        Message = message;
    }
}

public static class AppResponse
{
    public static AppResponse<T> Success<T>(T? data) =>
        new(data);
   
    public static AppResponse<T> Success<T>(string message, T? data) =>
        new(data, message);

    public static AppResponse<T> Failure<T>(AppError error) =>
        new(error);

    public static AppResponse<T> Failure<T>(string message, T? data = default) 
        => new(false, message, data);

    public static AppResponse<T> ValidationFailure<T>(
        IReadOnlyDictionary<string, List<string>> validationErrors)
    {
        ArgumentNullException.ThrowIfNull(validationErrors);

        var normalizedErrors = validationErrors.ToDictionary(
            entry => entry.Key,
            entry => entry.Value
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct(StringComparer.Ordinal)
                .ToArray());

        return Failure<T>(AppError.Validation(normalizedErrors));
    }

    public static AppResponse<T> ValidationFailure<T>(Dictionary<string, List<string>> validationErrors)
    {
        return ValidationFailure<T>((IReadOnlyDictionary<string, List<string>>)validationErrors);
    }
}

public sealed record AppError(ErrorType Type, string Code, string Message, IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
{
    public static AppError Validation(IReadOnlyDictionary<string, string[]> validationErrors)
    {
        ArgumentNullException.ThrowIfNull(validationErrors);

        return new AppError(
            ErrorType.Validation,
            ErrorCodes.Validation,
            "One or more validation failures have occurred.",
            validationErrors);
    }

    public static AppError Unexpected() =>
        new(
            ErrorType.Unexpected,
            ErrorCodes.Unexpected,
            "An unexpected error occurred.");
}

public static class ErrorCodes
{
    public const string Validation = "VALIDATION_ERROR";
    public const string Unexpected = "UNEXPECTED_ERROR";
}

public enum ErrorType
{
    Validation,
    NotFound,
    Unauthorized,
    Forbidden,
    Conflict,
    BusinessRule,
    DependencyUnavailable,
    Unexpected
}
