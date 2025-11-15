using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views;

public partial class ConfirmDeletionPage : ContentPage
{
    public ConfirmDeletionPage(ConfirmDeletionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}