using Rps.Api.Http.Shared.Dtos;

using System.Collections.Concurrent;

namespace Rps.Api.Http.Shared.Contracts.Interfaces;

/// <summary>
/// Marker interface implemented by <see cref="AppResponse{T}"/>.
/// Allows <see cref="Middleware.ValidationBehavior{TRequest,TResponse}"/>
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

