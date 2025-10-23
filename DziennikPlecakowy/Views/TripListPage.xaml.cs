using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views;

public partial class TripListPage : ContentPage
{
    public TripListPage(TripListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}