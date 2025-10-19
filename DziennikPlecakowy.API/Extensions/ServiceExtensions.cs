using DziennikPlecakowy.Infrastructure;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Services;
using DziennikPlecakowy.API.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace DziennikPlecakowy.API.Extensions;

public static class ServiceExtensions
{
    // Metoda do rejestracji wszystkich serwisów DI
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        //Health Check bazy danych
        services.AddHealthChecks();

        // Rejestracja MongoDB Context i Repozytoriów
        services.AddSingleton<IMongoDbContext, MongoDbContext>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IUserStatRepository, UserStatRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Rejestracja Serwisów Logiki Biznesowej
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IHashService, HashService>();
        services.AddScoped<ICypherService, CypherService>();
        services.AddScoped<ITripService, TripService>();

        return services;
    }

    // Metoda do konfiguracji JWT
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),

                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();
        return services;
    }

    // Metoda do konfiguracji Swaggera
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "DziennikPlecakowy API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Wprowadź token JWT po zalogowaniu",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });
        return services;
    }

    public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services)
    {
        services.AddHealthChecks()
                // Rejestracja naszego Health Checka dla MongoDB
                .AddCheck<MongoDBHealthCheck>("mongodb_ready", HealthStatus.Unhealthy, new string[] { "db", "ready" });

        return services;
    }
}