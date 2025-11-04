using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models;
using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.Repositories;
using System.Net.Http.Json;

namespace DziennikPlecakowy.Services.Local;
//Serwis synchronizacji danych z serwerem
public class SyncService
{
    private readonly LocalTripRepository _tripRepository;
    private readonly ApiClientService _apiClient;
    private readonly AuthService _authService;
    private static bool _isSyncing = false;

    public SyncService(
        LocalTripRepository tripRepository,
        ApiClientService apiClient,
        AuthService authService)
    {
        _tripRepository = tripRepository;
        _apiClient = apiClient;
        _authService = authService;
    }

    public async Task SynchronizePendingTripsAsync()
    {
        if (_isSyncing) return;

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            return; 
        }

        var userId = _authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        _isSyncing = true;

        try
        {
            var unsyncedTrips = await _tripRepository.GetUnsynchronizedTripsAsync();

            if (unsyncedTrips == null || !unsyncedTrips.Any())
            {
                _isSyncing = false;
                return;
            }

            foreach (var tripWithPoints in unsyncedTrips)
            {
                var requestDto = MapToRequestDTO(tripWithPoints);

                var response = await _apiClient.PostAsJsonAsync("/api/trip/addTrip", requestDto);

                if (response.IsSuccessStatusCode)
                {
                    var responseDto = await response.Content.ReadFromJsonAsync<TripAddResponseDTO>();

                    if (responseDto != null && !string.IsNullOrEmpty(responseDto.TripId))
                    {
                        await _tripRepository.MarkTripAsSynchronizedAsync(
                            tripWithPoints.Trip.LocalId,
                            responseDto.TripId);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private TripAddRequestDTO MapToRequestDTO(TripWithGeoPoints localData)
    {
        var trip = localData.Trip;

        var geoPointList = localData.GeoPoints.Select(p => new GeoPoint
        {
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            Height = p.Height,
            Timestamp = p.Timestamp
        }).ToArray();

        return new TripAddRequestDTO
        {
            Name = trip.Name,
            TripDate = trip.TripDate,
            Distance = trip.Distance,
            Duration = trip.Duration,
            ElevationGain = trip.ElevationGain,
            Steps = (int)trip.Steps,
            GeoPointList = geoPointList
        };
    }
}