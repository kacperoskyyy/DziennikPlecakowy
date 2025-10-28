#if ANDROID
using DziennikPlecakowy.Interfaces.Local;
using DziennikPlecakowy.Platforms.Android.Services;
#endif
using DziennikPlecakowy.Repositories;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.ViewModels;
using DziennikPlecakowy.Views;
using Microsoft.Extensions.Configuration;

namespace DziennikPlecakowy.Extensions;

public static class MauiProgramExtensions
{
    public static MauiAppBuilder RegisterDatabaseAndRepositories(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddTransient<LocalTripRepository>();
        builder.Services.AddTransient<TokenRepository>();
        return builder;
    }

    public static MauiAppBuilder RegisterAppServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<ApiClientService>();
        builder.Services.AddTransient<AuthService>();
        builder.Services.AddTransient<SyncService>();
        builder.Services.AddSingleton<TripTrackingService>();

#if ANDROID
        builder.Services.AddSingleton<IPedometerService, AndroidPedometerService>();
        builder.Services.AddSingleton<IPlatformNotificationService, PlatformNotificationService>();
#endif
        return builder;
    }

    public static MauiAppBuilder RegisterViewModelsAndPages(this MauiAppBuilder builder)
    {
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

        return builder;
    }

    public static MauiAppBuilder RegisterConfiguration(this MauiAppBuilder builder)
    {
        var assembly = typeof(MauiProgram).Assembly;

        var config = new ConfigurationBuilder()
                    .AddUserSecrets<App>()
                    .Build();

        builder.Configuration.AddConfiguration(config);
        builder.Services.AddSingleton<IConfiguration>(config);

        return builder;
    }
}