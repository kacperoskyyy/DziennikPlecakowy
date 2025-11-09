using DziennikPlecakowy.ViewModels;
using System.ComponentModel;

namespace DziennikPlecakowy.Views;

public partial class TripDetailPage : ContentPage
{
    private readonly TripDetailViewModel _viewModel;

    public TripDetailPage(TripDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        UpdateMapElements();
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (e.PropertyName == nameof(TripDetailViewModel.MapStartRegion) && _viewModel.MapStartRegion != null)
            {
                TripMap.MoveToRegion(_viewModel.MapStartRegion);
            }
            else if (e.PropertyName == nameof(TripDetailViewModel.RoutePolyline))
            {
                UpdateMapElements();
            }
        });
    }

    private void UpdateMapElements()
    {
        TripMap.MapElements.Clear();
        if (_viewModel.RoutePolyline != null)
        {
            TripMap.MapElements.Add(_viewModel.RoutePolyline);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }
}