using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views;

public partial class AdminPage : ContentPage
{
    public AdminPage(AdminViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}