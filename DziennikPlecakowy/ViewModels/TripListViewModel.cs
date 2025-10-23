using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.Repositories;
using DziennikPlecakowy.Services.Local;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace DziennikPlecakowy.ViewModels;

public partial class TripListViewModel : BaseViewModel
{
    private readonly LocalTripRepository _tripRepository;
    private readonly ApiClientService _apiClient;

    [ObservableProperty]
    ObservableCollection<LocalTrip> localTrips;
    [ObservableProperty]
    ObservableCollection<TripSummaryDTO> serverTrips;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowLocalList))]
    bool showServerList;

    public bool ShowLocalList => !ShowServerList;

    [ObservableProperty]
    object selectedTrip;

    public TripListViewModel(LocalTripRepository tripRepository, ApiClientService apiClient)
    {
        _tripRepository = tripRepository;
        _apiClient = apiClient;
        Title = "Moje Wycieczki";

        localTrips = new ObservableCollection<LocalTrip>();
        serverTrips = new ObservableCollection<TripSummaryDTO>();
    }

    [RelayCommand]
    private async Task LoadInitialTripsAsync()
    {
        if (ShowServerList) return;

        await LoadLocalTripsAsync();
    }


    private async Task LoadLocalTripsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            LocalTrips.Clear();
            var trips = await _tripRepository.GetLast50TripsAsync();
            foreach (var trip in trips)
            {
                LocalTrips.Add(trip);
            }
            ShowServerList = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadAllFromServerAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var response = await _apiClient.GetAsync("/api/Trip/getUserTripSummaries");

            if (response.IsSuccessStatusCode)
            {
                var trips = await response.Content.ReadFromJsonAsync<List<TripSummaryDTO>>();
                ServerTrips.Clear();
                foreach (var trip in trips)
                {
                    ServerTrips.Add(trip);
                }
                ShowServerList = true;
            }
            else
            {
                throw new Exception($"Nie udało się załadować wycieczek z serwera: {response}");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ShowLocalListAsync()
    {
        await LoadLocalTripsAsync();
    }

    [RelayCommand]
    private async Task GoToTripDetailAsync()
    {
        if (SelectedTrip == null)
            return;

        try
        {
            string localTripId = null;
            string serverTripId = null;

            if (SelectedTrip is LocalTrip localTrip)
            {
                localTripId = localTrip.LocalId.ToString();
                serverTripId = localTrip.ServerId;
            }
            else if (SelectedTrip is TripSummaryDTO serverTrip)
            {
                serverTripId = serverTrip.Id;
            }

            await Shell.Current.GoToAsync(
                $"{nameof(TripDetailPage)}?LocalTripId={localTripId}&ServerTripId={serverTripId}");
        }
        finally
        {
            SelectedTrip = null;
        }
    }

}
