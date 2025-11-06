using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using MongoDB.Driver;

namespace DziennikPlecakowy.API.Extensions;

public static class MiddlewareExtensions
{
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

        //app.UseHttpsRedirection();        //HTTPS
        app.UseRouting();

        app.UseHealthCheckEndpoint();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    public static WebApplication InitializeDatabaseIndexes(this WebApplication app)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();

            var userKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
            var userIndexModel = new CreateIndexModel<User>(userKeys, new CreateIndexOptions { Unique = true, Name = "EmailUniqueIndex" });
            dbContext.Users.Indexes.CreateOne(userIndexModel);

            var tripUserKeys = Builders<Trip>.IndexKeys.Ascending(t => t.UserId);
            var tripUserIndexModel = new CreateIndexModel<Trip>(tripUserKeys, new CreateIndexOptions { Name = "TripUserIdIndex" });
            dbContext.Trips.Indexes.CreateOne(tripUserIndexModel);

            var statsUserKeys = Builders<UserStat>.IndexKeys.Ascending(s => s.UserId);
            var statsUserIndexModel = new CreateIndexModel<UserStat>(statsUserKeys, new CreateIndexOptions { Unique = true, Name = "StatsUserIdIndex" });
            dbContext.UserStats.Indexes.CreateOne(statsUserIndexModel);
        });
        return app;
    }

    public static WebApplication UseHealthCheckEndpoint(this WebApplication app)
    {
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
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