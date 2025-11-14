#if ANDROID
using DziennikPlecakowy.Interfaces.Local;
using DziennikPlecakowy.Platforms.Android.Services;
#endif
using DziennikPlecakowy.Repositories;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.ViewModels;
using DziennikPlecakowy.Views;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using Microsoft.Extensions.Http;

namespace DziennikPlecakowy.Extensions;

public static class MauiProgramExtensions
{
    public static MauiAppBuilder RegisterDatabaseAndRepositories(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<LocalTripRepository>(sp =>
    new LocalTripRepository(
        sp.GetRequiredService<DatabaseService>(),
        sp.GetRequiredService<AuthService>()
    ));
        builder.Services.AddSingleton<TokenRepository>();
        return builder;
    }

    public static MauiAppBuilder RegisterAppServices(this MauiAppBuilder builder)
    {
        const string BaseApiUrl = "http://dziennikplecakowyapi.eu-central-1.elasticbeanstalk.com";


        builder.Services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri(BaseApiUrl);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (message, cert, chain, errors) => true
            };
            return new HttpClientHandler();
        });

        builder.Services.AddSingleton<ApiClientService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("ApiClient"); 
            var tokenRepo = sp.GetRequiredService<TokenRepository>();
            return new ApiClientService(httpClient, tokenRepo);
        });


        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<SyncService>();
        builder.Services.AddSingleton<TripTrackingService>();
        builder.Services.AddSingleton(Connectivity.Current);

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

        builder.Services.AddTransient<ChangePasswordViewModel>();
        builder.Services.AddTransient<ChangePasswordPage>();

        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<ForgotPasswordPage>();

        builder.Services.AddTransient<ResetPasswordViewModel>();
        builder.Services.AddTransient<ResetPasswordPage>();

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