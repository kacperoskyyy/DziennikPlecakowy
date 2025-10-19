using DziennikPlecakowy.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services
    .AddApplicationServices() // Rejestracja wszystkich Repozytoriów i Serwisów
    .AddJwtAuthentication(builder.Configuration) // Konfiguracja JWT i Autoryzacji
    .AddSwaggerDocumentation() // Konfiguracja Swaggera
    .AddLogging(); // Konfiguracja logów

var app = builder.Build();

app.InitializeDatabaseIndexes() // Tworzenie indeksów MongoDB przy starcie
   .AddApplicationMiddleware(); // Konfiguracja middleware (Swagger, Routing, Auth, itp.)

app.Run();