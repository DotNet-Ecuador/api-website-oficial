using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Infraestructure.Services;
using DotNetEcuador.API.Models;
using DotNetEcuador.API.Models.Auth;
using DotNetEcuador.API.Services.Auth;

namespace DotNetEcuador.API.Configuration;

public static class ServicesConfiguration
{
    public static void ConfigureApplicationServices(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<CommunityService>();
        services.AddScoped<IVolunteerApplicationService, VolunteerApplicationService>();
        services.AddScoped<IAreaOfInterestService, AreaOfInterestService>();

        // Register authentication services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();

    }
}