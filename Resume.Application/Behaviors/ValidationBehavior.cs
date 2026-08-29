using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Resume.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators for a request
/// before the request is dispatched to its handler.
/// </summary>
/// <typeparam name="TRequest">The type of the request being validated.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by the request.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Validates the request and throws a <see cref="ValidationException"/> if any rule fails.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="next">The delegate that invokes the next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response produced by the handler.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        List<IValidator<TRequest>> matchingValidators = validators.ToList();

        if (matchingValidators.Count == 0)
        {
            return await next();
        }

        ValidationContext<TRequest> context = new ValidationContext<TRequest>(request);

        ValidationResult[] results = await Task.WhenAll(
            matchingValidators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        List<ValidationFailure> failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
