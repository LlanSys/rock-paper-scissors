using FluentValidation;
using MediatR;

using Rps.Api.Http.Shared.Contracts.Interfaces;
using Rps.Api.Http.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rps.Api.Http.Shared.Middleware;

/// <summary>
/// MediatR pipeline behavior that runs all registered FluentValidation validators.
/// <para>
/// When <typeparamref name="TResponse"/> implements <see cref="IAppResponse"/>, validation
/// failures are returned directly as a typed failure response — <b>no exception is thrown</b>.
/// </para>
/// <para>
/// For any other response type, <see cref="ValidationException"/> is thrown as a fallback,
/// caught by <see cref="GlobalExceptionHandler"/> and mapped to a 400 ProblemDetails.
/// </para>
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>
    (IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IReadOnlyCollection<IValidator<TRequest>> _validators = validators.ToList();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (_validators.Count == 0)
        {
            return await next().ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)))
            .ConfigureAwait(false);

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next().ConfigureAwait(false);
        }

        // Fast path: if TResponse is AppResponse<T>, return a typed failure — no exception thrown.
        if (typeof(IAppResponse).IsAssignableFrom(typeof(TResponse)))
        {
            return (TResponse)IAppResponse.ValidationFailure(typeof(TResponse), failures);
        }

        // Fallback: throw for non-AppResponse handlers.
        // GlobalExceptionHandler maps this to 400 ProblemDetails.
        throw new ValidationException(failures);
    }
}
