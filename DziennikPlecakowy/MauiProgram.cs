#if ANDROID
using DziennikPlecakowy.Interfaces.Local;
using DziennikPlecakowy.Platforms.Android.Services;
#endif
using DziennikPlecakowy.Repositories;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.ViewModels;
using DziennikPlecakowy.Views;
using CommunityToolkit.Maui;

namespace DziennikPlecakowy;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit()
        .UseMauiMaps();
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
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<AccountViewModel>();
        builder.Services.AddTransient<AccountPage>();
        builder.Services.AddTransient<EditAccountViewModel>();
        builder.Services.AddTransient<EditAccountPage>();
        builder.Services.AddTransient<TripListViewModel>();
        builder.Services.AddTransient<TripListPage>();
        builder.Services.AddTransient<TripDetailViewModel>();
        builder.Services.AddTransient<TripDetailPage>();

        builder.Services.AddTransient<AdminViewModel>();
        builder.Services.AddTransient<AdminPage>();




        var app = builder.Build();
        var dbService = app.Services.GetService<DatabaseService>();
        dbService.InitializeDatabaseAsync().Wait();
        return app;
    }
}