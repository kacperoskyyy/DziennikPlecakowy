using DziennikPlecakowy.ViewModels;
using System.Collections.Specialized;

namespace DziennikPlecakowy.Views;

public partial class TripDetailPage : ContentPage
{
    private readonly TripDetailViewModel _viewModel;

    public TripDetailPage(TripDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Subskrybuj zdarzenie zmiany w³aœciwoœci w ViewModelu
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;

        // Od razu zaktualizuj mapê, jeœli dane ju¿ s¹
        UpdateMapElements();
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Reaguj na zmianê regionu mapy
            if (e.PropertyName == nameof(TripDetailViewModel.MapStartRegion) && _viewModel.MapStartRegion != null)
            {
                TripMap.MoveToRegion(_viewModel.MapStartRegion);
            }
            // Reaguj na zmianê linii trasy
            else if (e.PropertyName == nameof(TripDetailViewModel.RoutePolyline))
            {
                UpdateMapElements();
            }
        });
    }

    // Aktualizuje elementy mapy (Polyline)
    private void UpdateMapElements()
    {
        TripMap.MapElements.Clear();
        if (_viewModel.RoutePolyline != null)
        {
            TripMap.MapElements.Add(_viewModel.RoutePolyline);
        }
    }

    // Anuluj subskrypcje, gdy strona znika
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }
}