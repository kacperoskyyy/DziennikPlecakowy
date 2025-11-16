using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;

namespace DziennikPlecakowy
{
    public partial class App : Application
    {
        public App(AuthService authService)
        {
            InitializeComponent();
            MainPage = new AppShell();

            RegisterRoutes();
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await Shell.Current.GoToAsync(nameof(LoginPage), false);
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            //Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
           // Routing.RegisterRoute(nameof(AccountPage), typeof(AccountPage));
            Routing.RegisterRoute(nameof(EditAccountPage), typeof(EditAccountPage));
            //Routing.RegisterRoute(nameof(TripListPage), typeof(TripListPage));
            Routing.RegisterRoute(nameof(TripDetailPage), typeof(TripDetailPage));
            Routing.RegisterRoute(nameof(AdminPage), typeof(AdminPage));
            Routing.RegisterRoute(nameof(ChangePasswordPage), typeof(ChangePasswordPage));
            Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
            Routing.RegisterRoute(nameof(ResetPasswordPage), typeof(ResetPasswordPage));
            Routing.RegisterRoute(nameof(ConfirmDeletionPage), typeof(ConfirmDeletionPage));
        }
    }
}