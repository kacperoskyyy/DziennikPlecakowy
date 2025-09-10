using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace DziennikPlecakowy
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (Preferences.ContainsKey("auth_token"))
                MainPage = new AppShell();
            else
                MainPage = new NavigationPage(new Views.LoginPage());
        }
    }
}
