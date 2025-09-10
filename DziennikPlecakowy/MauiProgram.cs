using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Services;
using DziennikPlecakowy.ViewModels;
using DziennikPlecakowy.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace DziennikPlecakowy
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts => { /* opcjonalnie */ });

            // rejestracja serwisów mobilnych - implementacje interfejsów
            //builder.Services.AddSingleton<ApiServiceClient>();
            //builder.Services.AddSingleton<IAuthService, AuthServiceClient>();
            //builder.Services.AddSingleton<ITripService, TripServiceClient>();
            //builder.Services.AddSingleton<IUserService, UserServiceClient>();

            // viewmodels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<TripsListViewModel>();
            builder.Services.AddTransient<TripViewModel>();
            builder.Services.AddTransient<StatsViewModel>();

            // pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<TripsListPage>();
            builder.Services.AddTransient<TripPage>();
            builder.Services.AddTransient<StatsPage>();

            return builder.Build();
        }
    }
}
