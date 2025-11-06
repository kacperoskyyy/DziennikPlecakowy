using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Services.Local;
using System.Net.Http.Json;

namespace DziennikPlecakowy.ViewModels;

// ViewModel dla Dashboardu
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

        // Subskrybujemy zdarzenia TYLKO RAZ.
        // ViewModel będzie teraz stale nasłuchiwał zmian z serwisu.
        SubscribeToTrackingEvents();

        IsTracking = _tripTrackingService.IsTracking;
    }

    private void SubscribeToTrackingEvents()
    {
        // Ta metoda jest bezpieczna, usuwa starą subskrypcję (jeśli istnieje)
        // i dodaje nową, zapobiegając duplikatom.
        _tripTrackingService.OnTripDataUpdated -= OnTrackingDataUpdated;
        _tripTrackingService.OnTripDataUpdated += OnTrackingDataUpdated;
    }

    // Metoda Cleanup() została USUNIĘTA.
    // Już nie anulujemy subskrypcji przy opuszczaniu strony.

    private void OnTrackingDataUpdated(TripTrackingService.TrackingData data)
    {
        // Ta metoda będzie teraz wywoływana zawsze, gdy serwis wyśle
        // aktualizację, niezależnie od tego, czy strona Dashboard
        // jest widoczna, czy nie.
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentTripData = data;
            IsTracking = _tripTrackingService.IsTracking;
        });
    }

    [RelayCommand]
    private async Task LoadStatsAsync()
    {
        // USUNĘLIŚMY STĄD SubscribeToTrackingEvents().
        // Nie jest już potrzebne, bo subskrypcja jest stała.

        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var response = await _apiClient.GetAsync("/api/User/getUserStats");

            if (response.IsSuccessStatusCode)
            {
                UserProfile = await response.Content.ReadFromJsonAsync<UserProfileDTO>();
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
            bool success = await _tripTrackingService.StartTrackingAsync(TripName);
            if (success)
            {
                IsTracking = true;
                TripName = string.Empty;

                // Ustawiamy stan początkowy
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

        // Logika dialogu potwierdzającego
        bool result = await Shell.Current.DisplayAlert(
            "Zakończyć wycieczkę?",
            "Czy na pewno chcesz zatrzymać śledzenie?",
            "Tak, zakończ",
            "Anuluj");

        if (!result)
        {
            return; // Użytkownik anulował
        }

        // Użytkownik potwierdził
        try
        {
            await _tripTrackingService.StopTrackingAsync();

            // Odświeżamy statystyki globalne (pobieramy z serwera)
            await LoadStatsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Błąd", $"Nie udało się zatrzymać śledzenia: {ex.Message}", "OK");
        }
        finally
        {
            // Czyścimy UI po zatrzymaniu
            IsTracking = false;
            CurrentTripData = null;
        }
    }
}