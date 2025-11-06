using DziennikPlecakowy.ViewModels;
namespace DziennikPlecakowy.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;


    public DashboardPage(DashboardViewModel dashboardViewModel)
    {
        _viewModel = dashboardViewModel;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    //protected override void OnDisappearing()
    //{
    //    base.OnDisappearing();
    //    _viewModel.Cleanup();
    //}


}