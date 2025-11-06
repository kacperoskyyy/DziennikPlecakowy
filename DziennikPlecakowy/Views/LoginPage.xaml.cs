using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        BindingContext = viewModel;
    }
}