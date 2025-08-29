using DotNetEcuador.API.Models.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DotNetEcuador.API.Configuration;

public static class AuthConfiguration
{
    public static void ConfigureAuthentication(this IServiceCollection services)
    {
        // Get JWT settings from environment variables
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
                       throw new InvalidOperationException("JWT_SECRET_KEY environment variable is required");
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "DotNetEcuadorAPI";
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "DotNetEcuadorClients";

        // Configure JWT authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Set to true in production
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero // Remove delay of token when expire
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // Configure authorization with roles
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => 
                policy.RequireRole(UserRoles.Admin));
            
            options.AddPolicy("ModeratorOrAdmin", policy => 
                policy.RequireRole(UserRoles.Admin, UserRoles.Moderator));
            
            options.AddPolicy("AuthenticatedUser", policy => 
                policy.RequireAuthenticatedUser());
        });
    }
}