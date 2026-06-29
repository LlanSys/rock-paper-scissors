using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rps.Api.Http.Shared.Options;
using System.Reflection;
using Rps.Api.Http.Domain.Contracts.Interfaces;
using Rps.Api.Http.Domain.Contracts.Implementations;

namespace Rps.Api.Http.Shared.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GameOptions>()
            .Bind(configuration.GetSection(GameOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.Configure<MediatROptions>(configuration.GetSection(MediatROptions.SectionName));

        var mediatrOptions = configuration.GetSection(MediatROptions.SectionName).Get<MediatROptions>();

        services.AddMediatR(cfg =>
        {
            cfg.LicenseKey = mediatrOptions?.LicenseKey;
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(Middleware.ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddSingleton<IGameEngine, GameEngine>();
        services.AddSingleton<IComputerMoveSelector, ComputerMoveSelector>();

        services.AddExceptionHandler<Middleware.GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
