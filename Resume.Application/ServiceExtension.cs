using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Resume.Application.Behaviors;

namespace Resume.Application;

/// <summary>
/// Provides dependency injection extension methods for the application layer.
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Registers the application layer services, including MediatR handlers and FluentValidation validators.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(ServiceExtension).Assembly));
        services.AddValidatorsFromAssembly(typeof(ServiceExtension).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
