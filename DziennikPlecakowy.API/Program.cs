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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DziennikPlecakowy API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "WprowadŸ token JWT po zalogowaniu",
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


builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddAuthentication(options =>
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddSingleton<IMongoDbContext,MongoDbContext>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHashService, HashService>();
builder.Services.AddScoped<ICypherService, CypherService>();
builder.Services.AddScoped<ITripService, TripService>();

builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<IUserStatRepository, UserStatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();
app.Lifetime.ApplicationStarted.Register(() =>
{
    var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();

    // --- 1. Indeks dla U¿ytkowników (Kolekcja "Users") ---
    // Indeks: Email (Unikalny i Rosn¹cy) - Kluczowy dla logowania i rejestracji
    var userKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
    var userIndexModel = new CreateIndexModel<User>(userKeys, new CreateIndexOptions { Unique = true, Name = "EmailUniqueIndex" });
    dbContext.Users.Indexes.CreateOne(userIndexModel);

    // --- 2. Indeks dla Wycieczek (Kolekcja "Trips") ---
    // Indeks: UserId (Rosn¹cy) - Kluczowy dla szybkiego pobierania historii wycieczek
    var tripUserKeys = Builders<Trip>.IndexKeys.Ascending(t => t.UserId);
    var tripUserIndexModel = new CreateIndexModel<Trip>(tripUserKeys, new CreateIndexOptions { Name = "TripUserIdIndex" });
    dbContext.Trips.Indexes.CreateOne(tripUserIndexModel);

    // --- 3. Indeks dla Statystyk (Kolekcja "UserStats") ---
    // Indeks: UserId (Unikalny) - Kluczowy dla szybkiej aktualizacji statystyk
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
        c.OAuthClientId("swagger-ui");
        c.OAuthAppName("DziennikPlecakowy API - Swagger");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
