using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;

namespace DziennikPlecakowy
{
    public partial class App : Application
    {
        private readonly AuthService _authService;

        public App(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            base.OnStart();

            var authResult = await _authService.CheckAndRefreshTokenOnStartupAsync();

            if (authResult.IsSuccess)
            {

                await Shell.Current.GoToAsync($"//{nameof(DashboardPage)}");
            }
        }
    }
}