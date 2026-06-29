using Rps.Api.Http.Shared.Contracts.Interfaces;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Rps.Api.Http.Shared.Dtos;


/// <summary>
/// Envelope for all API responses. Construct via the static <see cref="AppResponse"/> factory methods.
/// The parameterless constructor is intentionally removed to prevent invalid state (IsSuccess=false, Error=null).
/// </summary>
public record AppResponse<T> : IAppResponse
{
    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public string? SessionId { get; init; }
    public T? Data { get; init; }

    // Failures are converted to ProblemDetails by GlobalExceptionHandler / BaseController.
    [JsonIgnore]
    public AppError? Error { get; }

    [JsonConstructor]
    private AppResponse(bool isSuccess, string? message, T? data)
    {
        IsSuccess = isSuccess;
        Message = message ?? "Operation Successful";
        Data = data;
    }

    internal AppResponse(T? data, string message = "Operation Successful")
    {
        IsSuccess = true;
        Data = data;
        Message = message;
    }

    internal AppResponse(AppError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        IsSuccess = false;
        Error = error;
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
