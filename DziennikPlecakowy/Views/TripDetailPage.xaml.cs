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

        // Subskrybuj zmianê kolekcji Pins
        if (_viewModel.Pins is INotifyCollectionChanged notifyingCollection)
        {
            notifyingCollection.CollectionChanged += Pins_CollectionChanged;
        }

        // Od razu zaktualizuj mapê, jeœli dane ju¿ s¹
        UpdateMapElements();
        UpdatePins();
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
            // Reaguj na zmianê ca³ej kolekcji Pinów
            else if (e.PropertyName == nameof(TripDetailViewModel.Pins))
            {
                // Po prostu odœwie¿ Piny, gdy ca³a kolekcja siê zmieni
                UpdatePins();
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

    // Aktualizuje Piny
    private void UpdatePins()
    {
        TripMap.Pins.Clear();
        if (_viewModel.Pins != null)
        {
            foreach (var pin in _viewModel.Pins)
            {
                TripMap.Pins.Add(pin);
            }
        }
    }

    // Reaguje na zmiany Wewn¹trz kolekcji Pins (np. dodanie pinu)
    private void Pins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdatePins);
    }

    // Anuluj subskrypcje, gdy strona znika
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        if (_viewModel.Pins is INotifyCollectionChanged notifyingCollection)
        {
            notifyingCollection.CollectionChanged -= Pins_CollectionChanged;
        }
    }
}