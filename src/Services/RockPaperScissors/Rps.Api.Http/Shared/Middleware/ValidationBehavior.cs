using FluentValidation;
using MediatR;
using Rps.Api.Http.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rps.Api.Http.Shared.Middleware;

public class ValidationBehavior<TRequest, TResponse>
    (IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (!_validators.Any())
        {
            return await next().ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))).ConfigureAwait(false);

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            var responseType = typeof(TResponse);

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(AppResponse<>))
            {
                var resultType = responseType.GetGenericArguments()[0];
                var validationErrorDict = failures
                    .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                    .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup
                    .ToList());

                var method = typeof(AppResponse)
                    .GetMethods()
                    .SingleOrDefault(method =>
                        method.Name == nameof(AppResponse.ValidationFailure) &&
                        method.IsGenericMethodDefinition &&
                        method.GetParameters() is [{ ParameterType: var parameterType }] &&
                        parameterType == typeof(Dictionary<string, List<string>>));

                if (method != null)
                {
                    return (TResponse)method.MakeGenericMethod(resultType).Invoke(null, [validationErrorDict])!;
                }
            }

            throw new ValidationException(failures);
        }

        return await next().ConfigureAwait(false);
    }
}
