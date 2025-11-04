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

            //MainPage = new AppShell();

            var authService = IPlatformApplication.Current.Services.GetService<AuthService>();

            var authResult = await authService.CheckAndRefreshTokenOnStartupAsync();

            if (authResult.IsSuccess)
            {
                if (authResult.MustChangePassword)
                {
                    await Shell.Current.GoToAsync(nameof(ChangePasswordPage));
                }
            }
            else
            {
                await Shell.Current.GoToAsync(nameof(LoginPage), false);
            }
        }
    }
}