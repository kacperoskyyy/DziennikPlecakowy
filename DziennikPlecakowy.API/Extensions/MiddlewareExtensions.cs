using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using MongoDB.Driver;

namespace DziennikPlecakowy.API.Extensions;

public static class MiddlewareExtensions
{
    // Metoda do rejestracji middleware i pipeline
    public static WebApplication AddApplicationMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DziennikPlecakowy API v1");
            });
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseHealthCheckEndpoint();

        // Uwierzytelnianie i Autoryzacja
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    // Metoda do inicjalizacji i tworzenia indeksów MongoDB
    public static WebApplication InitializeDatabaseIndexes(this WebApplication app)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();

            // 1. Indeks dla Użytkowników: Email (Unikalny)
            var userKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
            var userIndexModel = new CreateIndexModel<User>(userKeys, new CreateIndexOptions { Unique = true, Name = "EmailUniqueIndex" });
            dbContext.Users.Indexes.CreateOne(userIndexModel);

            // 2. Indeks dla Wycieczek: UserId
            var tripUserKeys = Builders<Trip>.IndexKeys.Ascending(t => t.UserId);
            var tripUserIndexModel = new CreateIndexModel<Trip>(tripUserKeys, new CreateIndexOptions { Name = "TripUserIdIndex" });
            dbContext.Trips.Indexes.CreateOne(tripUserIndexModel);

            // 3. Indeks dla Statystyk: UserId (Unikalny)
            var statsUserKeys = Builders<UserStat>.IndexKeys.Ascending(s => s.UserId);
            var statsUserIndexModel = new CreateIndexModel<UserStat>(statsUserKeys, new CreateIndexOptions { Unique = true, Name = "StatsUserIdIndex" });
            dbContext.UserStats.Indexes.CreateOne(statsUserIndexModel);
        });
        return app;
    }

    // Metoda do konfiguracji endpointu Health Check
    public static WebApplication UseHealthCheckEndpoint(this WebApplication app)
    {
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("HealthCheck"); // Tworzymy logger

                var mongoEntry = report.Entries.FirstOrDefault(e => e.Key == "mongodb_ready").Value;
                var mongoStatus = mongoEntry.Status.ToString();

                logger.LogInformation("Healthcheck result (MongoDB): Status={Status}, Duration={Duration}s",
                                      mongoStatus,
                                      mongoEntry.Duration.TotalSeconds);

                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        key = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        duration = e.Value.Duration.TotalSeconds
                    })
                });
                await context.Response.WriteAsync(result);
            }
        });
        return app;
    }
}