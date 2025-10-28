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

        SubscribeToTrackingEvents();

        IsTracking = _tripTrackingService.IsTracking;
    }

    private void SubscribeToTrackingEvents()
    {
        _tripTrackingService.OnTripDataUpdated += OnTrackingDataUpdated;
    }

    public void Cleanup()
    {
        _tripTrackingService.OnTripDataUpdated -= OnTrackingDataUpdated;
    }

    private void OnTrackingDataUpdated(TripTrackingService.TrackingData data)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentTripData = data;
            IsTracking = _tripTrackingService.IsTracking;
        });
    }

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
                Console.WriteLine("Failed to load user stats"); 
            }
        }
        catch (Exception ex)
        
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
            }
            else
            {
                Console.WriteLine("Failed to start tracking trip.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting trip tracking: {ex.Message}");
            return;
        }
        
    }

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