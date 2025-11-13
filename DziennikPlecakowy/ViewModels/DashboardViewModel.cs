using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Services.Local;
using System.Net.Http.Json;
using System.Collections.ObjectModel;
using Microsoft.Maui.Devices.Sensors;
using System.Text.Json; // Dodany using

namespace DziennikPlecakowy.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly TripTrackingService _tripTrackingService;
    private readonly AuthService _authService;
    private readonly ApiClientService _apiClient;

    [ObservableProperty]
    UserProfileDTO userProfile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotTracking))]
    bool isTracking;

    [ObservableProperty]
    TripTrackingService.TrackingData currentTripData;

    [ObservableProperty]
    string tripName;

    // USUNIĘTO: ActivePins, ActiveRoute, ActiveMapRegion

    public bool IsNotTracking => !IsTracking;

    public DashboardViewModel(
        TripTrackingService tripTrackingService,
        AuthService authService,
        ApiClientService apiClient)
    {
        _tripTrackingService = tripTrackingService;
        _authService = authService;
        _apiClient = apiClient;
        Title = "Dashboard";

        // USUNIĘTO: Inicjalizację ActivePins i ActiveRoute

        SubscribeToTrackingEvents();
        IsTracking = _tripTrackingService.IsTracking;
    }

    public void SubscribeToTrackingEvents()
    {
        _tripTrackingService.OnTripDataUpdated -= OnTrackingDataUpdated;
        _tripTrackingService.OnNewGeoPointAdded -= OnNewGeoPointAdded;

        _tripTrackingService.OnTripDataUpdated += OnTrackingDataUpdated;
        _tripTrackingService.OnNewGeoPointAdded += OnNewGeoPointAdded;
    }

    public void Cleanup()
    {
        _tripTrackingService.OnTripDataUpdated -= OnTrackingDataUpdated;
        _tripTrackingService.OnNewGeoPointAdded -= OnNewGeoPointAdded;
    }

    private void OnTrackingDataUpdated(TripTrackingService.TrackingData data)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentTripData = data;
            IsTracking = _tripTrackingService.IsTracking;
        });
    }

    private void OnNewGeoPointAdded(Location newLocation)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!IsTracking) return;
            // USUNIĘTO: Logikę dodawania do ActiveRoute, Pins i MapRegion
        });
    }

    [RelayCommand]
    private async Task LoadStatsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

            UserProfile = null;

        try
        {
            var response = await _apiClient.GetAsync("/api/User/getUserStats");

            if (response.IsSuccessStatusCode)
            {
                // Używamy ręcznej deserializacji z poprawnymi opcjami
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                UserProfile = JsonSerializer.Deserialize<UserProfileDTO>(jsonResponse, jsonOptions);

                _authService.SetCurrentUserProfile(UserProfile);
            }
            else
            {
                Console.WriteLine("Failed to load user stats");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading user stats: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task StartTrackingAsync()
    {
        if (IsTracking) return;
        try
        {
            // USUNIĘTO: Czyszczenie ActivePins, ActiveRoute, ActiveMapRegion

            bool success = await _tripTrackingService.StartTrackingAsync(TripName);
            if (success)
            {
                IsTracking = true;
                TripName = string.Empty;
                OnPropertyChanged(nameof(TripName));

                CurrentTripData = new TripTrackingService.TrackingData
                {
                    DistanceKm = 0,
                    Steps = 0,
                    Duration = TimeSpan.Zero
                };
            }
            else
            {
                await Shell.Current.DisplayAlert("Błąd", "Nie udzielono wszystkich uprawnień (Lokalizacja, Aktywność fizyczna) niezbędnych do śledzenia.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Błąd śledzenia", $"Wystąpił nieoczekiwany błąd: {ex.Message}", "OK");
            IsTracking = false;
        }
    }

    [RelayCommand]
    private async Task StopTrackingAsync()
    {
        if (!IsTracking) return;

        bool result = await Shell.Current.DisplayAlert(
            "Zakończyć wycieczkę?",
            "Czy na pewno chcesz zatrzymać śledzenie?",
            "Tak, zakończ",
            "Anuluj");

        if (!result)
        {
            return;
        }

        try
        {
            await _tripTrackingService.StopTrackingAsync();
            _authService.SetCurrentUserProfile(null);
            await LoadStatsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Błąd", $"Nie udało się zatrzymać śledzenia: {ex.Message}", "OK");
        }
        finally
        {
            IsTracking = false;
            CurrentTripData = null;
        }
    }
}