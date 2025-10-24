using DziennikPlecakowy.Views;

namespace DziennikPlecakowy;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
        Routing.RegisterRoute(nameof(AccountPage), typeof(AccountPage));
        Routing.RegisterRoute(nameof(EditAccountPage), typeof(EditAccountPage));
        Routing.RegisterRoute(nameof(TripListPage), typeof(TripListPage));
        Routing.RegisterRoute(nameof(TripDetailPage), typeof(TripDetailPage));
        Routing.RegisterRoute(nameof(AdminPage), typeof(AdminPage));
    }
}