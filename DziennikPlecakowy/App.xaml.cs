using DziennikPlecakowy.Views;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace DziennikPlecakowy
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App()
        {
            InitializeComponent();

            var builder = MauiApp.CreateBuilder();
            // ... konfiguracja DI
            var mauiApp = builder.Build();
            Services = mauiApp.Services;

            MainPage = new NavigationPage(new LoginPage());
        }
    }
}
