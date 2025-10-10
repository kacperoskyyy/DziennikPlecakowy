using DziennikPlecakowy.Infrastructure;
using DziennikPlecakowy.Infrastructure;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using DziennikPlecakowy.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Konfiguracja i Usługi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // ... Konfiguracja Swaggera (bez zmian)
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

// Rejestracja Usług (Repozytoria, Serwisy)
builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<IUserStatRepository, UserStatRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// WAŻNE: Naprawiona rejestracja cyklu (CypherService używa IServiceProvider)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHashService, HashService>();
builder.Services.AddScoped<ICypherService, CypherService>();
builder.Services.AddScoped<ITripService, TripService>();

// 2. Konfiguracja Uwierzytelniania JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Klucz musi być pobrany w postaci tablicy bajtów
    var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        // Używamy silnie typowanych wartości
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),

        ClockSkew = TimeSpan.Zero // Brak tolerancji czasu
    };
});

// 3. Konfiguracja Pipeline
var app = builder.Build();

// Rejestracja Indeksów MongoDB (Sekcja startowa)
app.Lifetime.ApplicationStarted.Register(() =>
{
    var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();

    // Indeks dla Użytkowników: Email (Unikalny)
    var userKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
    var userIndexModel = new CreateIndexModel<User>(userKeys, new CreateIndexOptions { Unique = true, Name = "EmailUniqueIndex" });
    dbContext.Users.Indexes.CreateOne(userIndexModel);

    // Indeks dla Wycieczek: UserId
    var tripUserKeys = Builders<Trip>.IndexKeys.Ascending(t => t.UserId);
    var tripUserIndexModel = new CreateIndexModel<Trip>(tripUserKeys, new CreateIndexOptions { Name = "TripUserIdIndex" });
    dbContext.Trips.Indexes.CreateOne(tripUserIndexModel);

    // Indeks dla Statystyk: UserId (Unikalny)
    var statsUserKeys = Builders<UserStat>.IndexKeys.Ascending(s => s.UserId);
    var statsUserIndexModel = new CreateIndexModel<UserStat>(statsUserKeys, new CreateIndexOptions { Unique = true, Name = "StatsUserIdIndex" });
    dbContext.UserStats.Indexes.CreateOne(statsUserIndexModel);
});


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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();