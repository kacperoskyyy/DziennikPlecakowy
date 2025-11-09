using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.Repositories;
using DziennikPlecakowy.Services.Local;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace DziennikPlecakowy.ViewModels;

[QueryProperty(nameof(LocalTripId), "LocalTripId")]
[QueryProperty(nameof(ServerTripId), "ServerTripId")]
public partial class TripDetailViewModel : BaseViewModel
{
    private readonly LocalTripRepository _tripRepository;
    private readonly ApiClientService _apiClient;

    [ObservableProperty]
    string localTripId;

    [ObservableProperty]
    string serverTripId;

    [ObservableProperty]
    TripDetailDTO tripDetails;

 
    [ObservableProperty]
    ObservableCollection<Pin> pins;

    [ObservableProperty]
    Microsoft.Maui.Controls.Maps.Polyline routePolyline;

    [ObservableProperty]
    MapSpan mapStartRegion;


    public IAsyncRelayCommand GoBackAsyncCommand { get; }
    public IAsyncRelayCommand DeleteTripCommand { get; }

    public TripDetailViewModel(LocalTripRepository tripRepository, ApiClientService apiClient)
    {
        _tripRepository = tripRepository;
        _apiClient = apiClient;
        Title = "Szczegóły Wycieczki";
        Pins = new ObservableCollection<Pin>();

        GoBackAsyncCommand = new AsyncRelayCommand(GoBackAsync);
        DeleteTripCommand = new AsyncRelayCommand(DeleteTripAsync);
    }

    partial void OnLocalTripIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
            LoadTripDataCommand.Execute(null);
    }

    partial void OnServerTripIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
            LoadTripDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadTripDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        TripDetails = null;
        Pins.Clear();
        RoutePolyline = null;
        MapStartRegion = null;

        try
        {
            if (long.TryParse(LocalTripId, out long localId))
            {
                var localData = await _tripRepository.GetTripWithGeoPointsAsync(localId);
                if (localData != null)
                {
                    TripDetails = MapLocalToDetailDTO(localData);
                }
            }
            else if (!string.IsNullOrEmpty(ServerTripId))
            {
                var response = await _apiClient.GetAsync($"/api/Trip/{ServerTripId}");
                if (response.IsSuccessStatusCode)
                {
                    TripDetails = await response.Content.ReadFromJsonAsync<TripDetailDTO>();
                }
            }

            if (TripDetails != null)
            {
                PrepareMapData();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void PrepareMapData()
    {
        if (TripDetails.GeoPointList == null || !TripDetails.GeoPointList.Any())
            return;

        var points = TripDetails.GeoPointList;

        var polyline = new Microsoft.Maui.Controls.Maps.Polyline
        {
            StrokeColor = Colors.Blue,
            StrokeWidth = 5
        };
        foreach (var point in points)
        {
            polyline.Geopath.Add(new Location(point.Latitude, point.Longitude));
        }
        RoutePolyline = polyline;

        var startPoint = points.First();
        var endPoint = points.Last();

        Pins.Add(new Pin
        {
            Label = "Start",
            Location = new Location(startPoint.Latitude, startPoint.Longitude),
            Type = PinType.Place
        });
        Pins.Add(new Pin
        {
            Label = "Koniec",
            Location = new Location(endPoint.Latitude, endPoint.Longitude),
            Type = PinType.Place
        });

        MapStartRegion = MapSpan.FromCenterAndRadius(
            new Location(startPoint.Latitude, startPoint.Longitude),
            Distance.FromKilometers(1));
    }

    private TripDetailDTO MapLocalToDetailDTO(TripWithGeoPoints localData)
    {
        return new TripDetailDTO
        {
            Id = localData.Trip.ServerId,
            Name = localData.Trip.Name,
            TripDate = localData.Trip.TripDate,
            Distance = localData.Trip.Distance,
            Duration = localData.Trip.Duration,
            ElevationGain = localData.Trip.ElevationGain,
            Steps = (int)localData.Trip.Steps,
            GeoPointList = localData.GeoPoints.Select(p => new GeoPointDTO
            {
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Height = p.Height,
                Timestamp = p.Timestamp
            }).ToList()
        };
    }

    private async Task GoBackAsync()
    {
        if (IsBusy) return;
        try { await Shell.Current.GoToAsync(".."); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Błąd nawigacji: {ex.Message}"); }
    }

    private async Task DeleteTripAsync()
    {
        if (IsBusy) return;

        bool confirmed = await Shell.Current.DisplayAlert(
            "Potwierdź usunięcie",
            "Czy na pewno chcesz nieodwracalnie usunąć tę wycieczkę?",
            "Tak, usuń",
            "Anuluj"
        );

        if (!confirmed) return;

        IsBusy = true;
        bool deleteSuccess = false;
        string apiError = string.Empty;

        try
        {
            if (TripDetails != null && !string.IsNullOrEmpty(TripDetails.Id))
            {
                var response = await _apiClient.DeleteAsync($"/api/Trip/{TripDetails.Id}");
                if (!response.IsSuccessStatusCode)
                {
                    apiError = $"Błąd API: {response.StatusCode}.";
                }
            }

            if (long.TryParse(LocalTripId, out long localId))
            {
                await _tripRepository.DeleteTripAsync(localId);
            }

            if (string.IsNullOrEmpty(apiError))
            {
                deleteSuccess = true;
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Błąd", $"Wystąpił wyjątek: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }

        if (deleteSuccess)
        {
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await Shell.Current.DisplayAlert("Błąd", $"Nie udało się usunąć wycieczki. {apiError}", "OK");
        }
    }
}