using DziennikPlecakowy.Interfaces.Local;
#if ANDROID
using DziennikPlecakowy.Platforms.Android.Services;
#endif
using DziennikPlecakowy.Repositories;
using DziennikPlecakowy.Services.Local;
namespace DziennikPlecakowy;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        // Rejestracje Bazy i Repo
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddTransient<LocalTripRepository>();
        builder.Services.AddTransient<TokenRepository>();

        // Rejestracje Serwisów Aplikacji
        builder.Services.AddSingleton<ApiClientService>();
        builder.Services.AddTransient<AuthService>();
        builder.Services.AddTransient<SyncService>();

        builder.Services.AddSingleton<TripTrackingService>();

#if ANDROID
        builder.Services.AddSingleton<IPedometerService, AndroidPedometerService>();
        builder.Services.AddSingleton<IPlatformNotificationService, PlatformNotificationService>();
#endif

        var app = builder.Build();
        var dbService = app.Services.GetService<DatabaseService>();

        dbService.InitializeDatabaseAsync().Wait();


        return app;
    }
}