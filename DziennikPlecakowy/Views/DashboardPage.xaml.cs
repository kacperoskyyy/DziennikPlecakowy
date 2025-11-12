using DziennikPlecakowy.ViewModels;
using System.ComponentModel;

namespace DziennikPlecakowy.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage(DashboardViewModel dashboardViewModel)
    {
        _viewModel = dashboardViewModel;
        BindingContext = _viewModel;
        InitializeComponent();

        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.SubscribeToTrackingEvents();
        // USUNIÊTO: UpdateMapElements();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.Cleanup();
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // USUNIÊTO: Ca³¹ logikê MoveToRegion i UpdateMapElements
    }

    // USUNIÊTO: Ca³¹ metodê UpdateMapElements()
}