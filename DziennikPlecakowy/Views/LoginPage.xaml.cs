using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        BindingContext = viewModel;
        _viewModel = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel?.CheckAutoLoginCommand.CanExecute(null) ?? false)
        {
            _viewModel.CheckAutoLoginCommand.Execute(null);
        }
    }
}