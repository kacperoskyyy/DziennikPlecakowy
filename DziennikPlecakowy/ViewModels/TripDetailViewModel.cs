using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO.Local;
using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.Repositories;
using DziennikPlecakowy.Services.Local;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace DziennikPlecakowy.ViewModels
{
    // Ten atrybut pozwala ViewModelowi odbierać parametry z nawigacji
    [QueryProperty(nameof(LocalTripId), "LocalTripId")]
    [QueryProperty(nameof(ServerTripId), "ServerTripId")]
    public partial class TripDetailViewModel : BaseViewModel
    {
        private readonly LocalTripRepository _tripRepository;
        private readonly ApiClientService _apiClient;

        // --- Parametry z Nawigacji ---
        [ObservableProperty]
        string localTripId;

        [ObservableProperty]
        string serverTripId;

        // --- Dane Wycieczki (do bindowania statystyk) ---
        [ObservableProperty]
        TripDetailDTO tripDetails; // Używamy DTO jako wspólnego modelu dla widoku

        // --- Dane dla Mapy ---
        [ObservableProperty]
        ObservableCollection<Pin> pins; // Piny (start i koniec)

        [ObservableProperty]
        Microsoft.Maui.Controls.Maps.Polyline routePolyline; // Linia trasy

        [ObservableProperty]
        MapSpan mapStartRegion; // Region mapy do wycentrowania

        public TripDetailViewModel(LocalTripRepository tripRepository, ApiClientService apiClient)
        {
            _tripRepository = tripRepository;
            _apiClient = apiClient;
            Title = "Szczegóły Wycieczki";
            Pins = new ObservableCollection<Pin>();
        }

        // Ta metoda odpali się automatycznie, gdy parametry nawigacji się zmienią
        partial void OnLocalTripIdChanged(string value)
        {
            // Odpalamy ładowanie, jak tylko dostaniemy ID
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

            try
            {
                // Priorytet: Załaduj z lokalnej bazy, jeśli masz LocalTripId
                if (long.TryParse(LocalTripId, out long localId))
                {
                    var localData = await _tripRepository.GetTripWithGeoPointsAsync(localId);
                    if (localData != null)
                    {
                        TripDetails = MapLocalToDetailDTO(localData);
                    }
                }
                // Fallback: Jeśli nie ma lokalnie (lub to był ServerTrip), ładuj z API
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
                    // Mamy dane, przygotuj mapę
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

            // 1. Stwórz Polyline (trasę)
            var polyline = new Microsoft.Maui.Controls.Maps.Polyline
            {
                StrokeColor = Colors.Blue,
                StrokeWidth = 5
            };
            foreach (var point in points)
            {
                polyline.Geopath.Add(new Location(point.Latitude, point.Longitude));
            }
            RoutePolyline = polyline; // Ustaw właściwość

            // 2. Stwórz Piny (Start i Koniec)
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

            // 3. Ustaw region startowy mapy
            MapStartRegion = MapSpan.FromCenterAndRadius(
                new Location(startPoint.Latitude, startPoint.Longitude),
                Distance.FromKilometers(1));
        }

        // Mapper, żeby widok zawsze działał na tym samym obiekcie (TripDetailDTO)
        private TripDetailDTO MapLocalToDetailDTO(TripWithGeoPoints localData)
        {
            return new TripDetailDTO
            {
                Id = localData.Trip.ServerId, // Może być nullem
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
    }
}