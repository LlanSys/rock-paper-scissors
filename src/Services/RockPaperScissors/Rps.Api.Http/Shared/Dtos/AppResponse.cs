using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Rps.Api.Http.Shared.Dtos;

/// <summary>
/// Marker interface implemented by <see cref="AppResponse{T}"/>.
/// Allows <see cref="Rps.Api.Http.Shared.Middleware.ValidationBehavior{TRequest,TResponse}"/>
/// to construct a typed validation failure without throwing exceptions or using ad-hoc reflection.
/// </summary>
public interface IAppResponse
{
    /// <summary>
    /// Runtime dispatch: given a concrete <paramref name="responseType"/> (must be <c>AppResponse&lt;T&gt;</c>)
    /// and a list of FluentValidation failures, creates and returns a typed validation-failure response.
    /// </summary>
    internal static IAppResponse ValidationFailure(
        Type responseType,
        IReadOnlyList<FluentValidation.Results.ValidationFailure> failures)
    {
        var factory = _factoryCache.GetOrAdd(responseType, BuildFactory);
        return factory(failures);
    }

    // Cache of compiled factory delegates keyed by concrete AppResponse<T> type.
    private static readonly ConcurrentDictionary<Type, Func<IReadOnlyList<FluentValidation.Results.ValidationFailure>, IAppResponse>>
        _factoryCache = new();

    private static Func<IReadOnlyList<FluentValidation.Results.ValidationFailure>, IAppResponse> BuildFactory(Type responseType)
    {
        // responseType is AppResponse<T>; extract T and create a closed generic helper.
        var innerType = responseType.GetGenericArguments()[0];
        var helperMethod = typeof(IAppResponse)
            .GetMethod(nameof(BuildFailure), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(innerType);

        return failures => (IAppResponse)helperMethod.Invoke(null, [failures])!;
    }

    private static AppResponse<T> BuildFailure<T>(IReadOnlyList<FluentValidation.Results.ValidationFailure> failures)
    {
        var errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => (string[])g.Select(f => f.ErrorMessage)
                                 .Where(m => !string.IsNullOrWhiteSpace(m))
                                 .Distinct(StringComparer.Ordinal)
                                 .ToArray());

        return new AppResponse<T>(AppError.Validation(errors));
    }
}

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
