using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rps.Api.Http.Shared.Middleware;

/// <summary>
/// MediatR pipeline behavior that runs all registered FluentValidation validators for a request.
/// On failure, throws <see cref="ValidationException"/> which is caught by the global
/// <see cref="GlobalExceptionHandler"/> and mapped to a 400 ProblemDetails response.
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

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next().ConfigureAwait(false);
    }
}
