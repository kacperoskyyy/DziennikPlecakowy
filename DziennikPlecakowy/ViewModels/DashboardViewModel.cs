using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Services.Local;
using System.Net.Http.Json;

namespace DziennikPlecakowy.ViewModels
{
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

            SubscribeToTrackingEvents();

            IsTracking = _tripTrackingService.IsTracking;
        }

        // Metoda do subskrypcji eventów
        private void SubscribeToTrackingEvents()
        {
            _tripTrackingService.OnTripDataUpdated += OnTrackingDataUpdated;
        }

        // Metoda do anulowania subskrypcji (ważne!)
        public void Cleanup()
        {
            _tripTrackingService.OnTripDataUpdated -= OnTrackingDataUpdated;
        }

        // Event handler - odpala się, gdy serwis śledzenia wysyła dane
        private void OnTrackingDataUpdated(TripTrackingService.TrackingData data)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CurrentTripData = data;
                IsTracking = _tripTrackingService.IsTracking;
            });
        }

        // Komenda odpalana, gdy strona się pojawia
        [RelayCommand]
        private async Task LoadStatsAsync()
        {
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
                    // TODO: Obsługa błędu pobierania statystyk
                }
            }
            catch (Exception ex)
            {
                // TODO: Obsługa wyjątku
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Komenda Start
        [RelayCommand]
        private async Task StartTrackingAsync()
        {
            if (IsTracking) return;

            bool success = await _tripTrackingService.StartTrackingAsync(TripName);
            if (success)
            {
                IsTracking = true;
                TripName = string.Empty;
            }
            else
            {
                // TODO: Pokaż błąd (np. brak uprawnień GPS)
            }
        }

        // Komenda Stop (spełnia wymóg 3-sekundowego przytrzymania)
        [RelayCommand]
        private async Task StopTrackingAsync()
        {
            if (!IsTracking) return;

            await _tripTrackingService.StopTrackingAsync();
            IsTracking = false;
            CurrentTripData = null;


            await LoadStatsAsync();
        }
    }
}