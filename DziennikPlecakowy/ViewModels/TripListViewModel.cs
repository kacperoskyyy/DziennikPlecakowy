using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.Repositories;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace DziennikPlecakowy.ViewModels;

public partial class TripListViewModel : BaseViewModel
{
    private readonly LocalTripRepository _tripRepository;
    private readonly ApiClientService _apiClient;
    private readonly IConnectivity _connectivity; 

    [ObservableProperty]
    ObservableCollection<LocalTrip> trips;



    [ObservableProperty]
    object selectedTrip;


    public TripListViewModel(LocalTripRepository tripRepository, ApiClientService apiClient, IConnectivity connectivity)
    {
        _tripRepository = tripRepository;
        _apiClient = apiClient;
        _connectivity = connectivity; 
        Title = "Moje Wycieczki";

        Trips = new ObservableCollection<LocalTrip>();
    }

    [RelayCommand]
    private async Task LoadTripsAsync() 
    { 
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            await SynchronizeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd synchronizacji: {ex.Message}");
        }
        finally
        {
            await LoadTripsFromLocalDbAsync();
            IsBusy = false;
        }
    }

    private async Task LoadTripsFromLocalDbAsync()
    {
        try
        {
            Trips.Clear();
            var localTrips = await _tripRepository.GetTripsForUserAsync();
            foreach (var trip in localTrips)
            {
                Trips.Add(trip);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Błąd", $"Nie udało się załadować lokalnych wycieczek: {ex.Message}", "OK");
        }
    }

    private async Task SynchronizeAsync()
    {
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            await Shell.Current.DisplayAlert("Brak sieci", "Brak połączenia z internetem. Wyświetlanie danych lokalnych.", "OK");
            return;
        }

        var unsyncedTrips = await _tripRepository.GetUnsynchronizedTripsAsync();
        foreach (var tripWithPoints in unsyncedTrips)
        {
            var createDto = MapLocalToCreateDTO(tripWithPoints);
            var response = await _apiClient.PostAsJsonAsync("/api/Trip", createDto);

            if (response.IsSuccessStatusCode)
            {
                var createdTrip = await response.Content.ReadFromJsonAsync<TripDetailDTO>();
                if (createdTrip != null && !string.IsNullOrEmpty(createdTrip.Id))
                {
                    await _tripRepository.MarkTripAsSynchronizedAsync(tripWithPoints.Trip.LocalId, createdTrip.Id);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Nie udało się wysłać wycieczki {tripWithPoints.Trip.LocalId}");
            }
        }

        var serverResponse = await _apiClient.GetAsync("/api/Trip/getUserTripSummaries");
        if (serverResponse.IsSuccessStatusCode)
        {
            var serverTrips = await serverResponse.Content.ReadFromJsonAsync<List<TripSummaryDTO>>();
            foreach (var summary in serverTrips)
            {
                await _tripRepository.UpsertTripFromServerAsync(summary);
            }
        }
    }

    private TripDetailDTO MapLocalToCreateDTO(TripWithGeoPoints localData)
    {
        return new TripDetailDTO
        {
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


    [RelayCommand]
    private async Task GoToTripDetailAsync(object selectedItem)
    {
        if (selectedItem == null || selectedItem is not LocalTrip localTrip)
            return;

        try
        {
            await Shell.Current.GoToAsync(
                $"{nameof(TripDetailPage)}?LocalTripId={localTrip.LocalId}&ServerTripId={localTrip.ServerId}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Błąd nawigacji", ex.Message, "OK");
        }
    }

    async partial void OnSelectedTripChanged(object value)
    {
        if (value == null)
            return;

        await GoToTripDetailAsync(value);
        SelectedTrip = null;
    }
}