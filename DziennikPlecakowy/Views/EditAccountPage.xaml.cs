using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views;

public partial class EditAccountPage : ContentPage
{
    public EditAccountPage(EditAccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}