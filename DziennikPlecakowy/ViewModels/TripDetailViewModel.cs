using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.Repositories;
using DziennikPlecakowy.Services.Local;
using System.Collections.ObjectModel;
using System.Text.Json;
// USUNIĘTO: using Microsoft.Maui.Controls.Maps;
// USUNIĘTO: using Microsoft.Maui.Maps;

namespace DziennikPlecakowy.ViewModels;

[QueryProperty(nameof(LocalTripId), "LocalTripId")]
[QueryProperty(nameof(ServerTripId), "ServerTripId")]
public partial class TripDetailViewModel : BaseViewModel
{
    private readonly LocalTripRepository _tripRepository;
    private readonly ApiClientService _apiClient;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [ObservableProperty]
    string localTripId;

    [ObservableProperty]
    string serverTripId;

    [ObservableProperty]
    TripDetailDTO tripDetails;

    // USUNIĘTO: pins, routePolyline, mapStartRegion

    // Ta właściwość jest dla WebView (Leaflet) i zostaje
    [ObservableProperty]
    IDictionary<string, object> mapParameters;

    public IAsyncRelayCommand GoBackAsyncCommand { get; }
    public IAsyncRelayCommand DeleteTripCommand { get; }

    public TripDetailViewModel(LocalTripRepository tripRepository, ApiClientService apiClient)
    {
        _tripRepository = tripRepository;
        _apiClient = apiClient;
        Title = "Szczegóły Wycieczki";
        // USUNIĘTO: Inicjalizację Pins

        GoBackAsyncCommand = new AsyncRelayCommand(GoBackAsync);
        DeleteTripCommand = new AsyncRelayCommand(DeleteTripAsync);
    }

    partial void OnServerTripIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            LoadTripDataCommand.Execute(null);
        }
    }

    partial void OnLocalTripIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(ServerTripId))
        {
            return;
        }
    }


    [RelayCommand]
    private async Task LoadTripDataAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrEmpty(ServerTripId))
        {
            await Shell.Current.DisplayAlert("Błąd", "Nie można załadować wycieczki, brak ServerId.", "OK");
            return;
        }

        IsBusy = true;
        TripDetails = null;

        // USUNIĘTO: Czyszczenie Pins, RoutePolyline, MapStartRegion

        try
        {
            var response = await _apiClient.GetAsync($"/api/Trip/{ServerTripId}");

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                TripDetails = JsonSerializer.Deserialize<TripDetailDTO>(jsonResponse, _jsonOptions);
            }
            else
            {
                await Shell.Current.DisplayAlert("Błąd API", $"Nie udało się pobrać danych: {response.StatusCode}", "OK");
            }

            // USUNIĘTO: Wywołanie PrepareMapData()
            // (Logika mapowania jest teraz w OnTripDetailsChanged)
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Błąd krytyczny", $"Wystąpił wyjątek: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // USUNIĘTO: Całą metodę PrepareMapData()

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
            if (!string.IsNullOrEmpty(ServerTripId))
            {
                var response = await _apiClient.DeleteAsync($"/api/Trip/delete/{ServerTripId}");
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

    // Ta metoda jest kluczowa dla mapy WebView i zostaje
    partial void OnTripDetailsChanged(TripDetailDTO value)
    {
        MapParameters = new Dictionary<string, object>
        {
            { "GeoPointList", value?.GeoPointList }
        };
    }
}