using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Repositories;
using DziennikPlecakowy.Services.Local;
using System.Text.Json;

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

    [ObservableProperty]
    IDictionary<string, object> mapParameters;

    [ObservableProperty]
    double averageSpeed;

    [ObservableProperty]
    double averagePace;

    [ObservableProperty]
    double maxSpeed;

    [ObservableProperty]
    double fastestPace;

    [ObservableProperty]
    double elevationLoss;

    [ObservableProperty]
    double maxAltitude;

    [ObservableProperty]
    double minAltitude;

    public IAsyncRelayCommand GoBackAsyncCommand { get; }
    public IAsyncRelayCommand DeleteTripCommand { get; }

    public TripDetailViewModel(LocalTripRepository tripRepository, ApiClientService apiClient)
    {
        _tripRepository = tripRepository;
        _apiClient = apiClient;
        Title = "Szczegóły Wycieczki";
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

    partial void OnTripDetailsChanged(TripDetailDTO value)
    {
        MapParameters = new Dictionary<string, object>
        {
            { "GeoPointList", value?.GeoPointList }
        };

        if (value != null && value.Duration > 0)
        {
            AverageSpeed = value.Distance / (value.Duration / 3600.0);
            AveragePace = (AverageSpeed > 0) ? (60.0 / AverageSpeed) : 0;
        }
        else
        {
            AverageSpeed = 0;
            AveragePace = 0;
        }

        MaxSpeed = 0;
        FastestPace = 0;
        MaxAltitude = 0;
        MinAltitude = 0;

        if (value?.GeoPointList != null && value.GeoPointList.Any())
        {
            var pointsCopy = new List<GeoPointDTO>(value.GeoPointList);
            Task.Run(() => CalculateAdvancedStats(pointsCopy));
        }
    }


    private void CalculateAdvancedStats(List<GeoPointDTO> geoPoints)
    {
        if (geoPoints.Count < 2) return;

        double tempMaxSpeed = 0;
        double tempMaxAltitude = double.MinValue;
        double tempMinAltitude = double.MaxValue;

        var points = geoPoints.OrderBy(p => p.Timestamp).ToList();

        if (points.Any())
        {
            tempMaxAltitude = points.First().Height;
            tempMinAltitude = points.First().Height;
        }

        for (int i = 1; i < points.Count; i++)
        {
            var p1 = points[i - 1];
            var p2 = points[i];

            var timeDiffSeconds = (p2.Timestamp - p1.Timestamp).TotalSeconds;
            if (timeDiffSeconds <= 0) continue;

            double distanceKm = GetDistance(p1.Latitude, p1.Longitude, p2.Latitude, p2.Longitude);

            if (distanceKm > 0)
            {
                double speedKmh = distanceKm / (timeDiffSeconds / 3600.0);
                if (speedKmh > tempMaxSpeed && speedKmh < 80.0)
                {
                    tempMaxSpeed = speedKmh;
                }
            }
        }

        MaxSpeed = tempMaxSpeed;
        FastestPace = (tempMaxSpeed > 0) ? (60.0 / tempMaxSpeed) : 0;
        MaxAltitude = tempMaxAltitude;
        MinAltitude = tempMinAltitude;
    }

    private double GetDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0;

        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);
        double rLat1 = ToRadians(lat1);
        double rLat2 = ToRadians(lat2);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(rLat1) * Math.Cos(rLat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        return R * c;
    }

    private double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }
}