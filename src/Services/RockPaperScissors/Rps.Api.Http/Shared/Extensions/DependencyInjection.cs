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
        // Options Setup
        services.Configure<GameOptions>(configuration.GetSection(GameOptions.SectionName));
        services.Configure<MediatROptions>(configuration.GetSection(MediatROptions.SectionName));

        var mediatrOptions = configuration.GetSection(MediatROptions.SectionName).Get<MediatROptions>();

        // MediatR Setup
        services.AddMediatR(cfg =>
        {
            cfg.LicenseKey = mediatrOptions?.LicenseKey;
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(Middleware.ValidationBehavior<,>));
        });

        // FluentValidation Setup
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Domain Services
        services.AddScoped<IGameEngine, GameEngine>();
        services.AddScoped<IComputerMoveSelector, ComputerMoveSelector>();

        return services;
    }
}
