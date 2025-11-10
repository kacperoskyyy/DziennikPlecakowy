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
        UpdateMapElements();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.Cleanup();
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DashboardViewModel.ActiveMapRegion) && _viewModel.ActiveMapRegion != null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ActiveTripMap.MoveToRegion(_viewModel.ActiveMapRegion);
            });
        }
        else if (e.PropertyName == nameof(DashboardViewModel.ActiveRoute))
        {
            MainThread.BeginInvokeOnMainThread(UpdateMapElements);
        }
    }

    private void UpdateMapElements()
    {
        ActiveTripMap.MapElements.Clear();
        if (_viewModel.ActiveRoute != null)
        {
            ActiveTripMap.MapElements.Add(_viewModel.ActiveRoute);
        }
    }
}