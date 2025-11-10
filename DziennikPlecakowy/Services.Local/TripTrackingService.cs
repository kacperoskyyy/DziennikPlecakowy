using DziennikPlecakowy.Interfaces.Local;
using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.PermissionsApp;
using DziennikPlecakowy.Repositories;

namespace DziennikPlecakowy.Services.Local;
// Serwis śledzenia wycieczek

public class TripTrackingService
{
    private readonly LocalTripRepository _tripRepository;
    private readonly IPlatformNotificationService _notificationService;
    private readonly AuthService _authService;
    private readonly SyncService _syncService;
    private readonly IPedometerService _pedometerService;

    private bool _isRunning = false;
    private LocalTrip _currentTrip;
    private Location _lastSavedLocation;
    private DateTime _startTime;
    private double _totalDistanceKm = 0.0;
    private long _currentSteps = 0;
    private System.Threading.Timer _autoStopTimer;
    private System.Threading.Timer _uiUpdateTimer;

    public event Action<TrackingData> OnTripDataUpdated;
    public event Action<Location> OnNewGeoPointAdded;
    public bool IsTracking => _isRunning;

    private const double MinDistanceThresholdMeters = 15.0;

    public TripTrackingService(
        LocalTripRepository tripRepository,
        IPlatformNotificationService notificationService,
        AuthService authService,
        SyncService syncService,
        IPedometerService pedometerService)
    {
        _tripRepository = tripRepository;
        _notificationService = notificationService;
        _authService = authService;
        _syncService = syncService;
        _pedometerService = pedometerService;
    }

    public async Task<bool> StartTrackingAsync(string tripName)
    {
        if (_isRunning) return true;

        if (!await CheckPermissionsAsync())
        {
            return false;
        }

        var userId = _authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        _startTime = DateTime.UtcNow;
        _currentTrip = new LocalTrip
        {
            UserId = userId,
            Name = string.IsNullOrEmpty(tripName) ? $"Wycieczka z {_startTime:dd.MM.yyyy}" : tripName,
            TripDate = _startTime,
            IsSynchronized = false
        };

        await _tripRepository.SaveTripAsync(_currentTrip, new List<LocalGeoPoint>());

        _totalDistanceKm = 0.0;
        _currentSteps = 0;
        _lastSavedLocation = null;

        StartPedometer();
        await StartGeolocation();

        _notificationService.StartForegroundService("Dziennik Plecakowy", "Rozpoczęto śledzenie wycieczki...");

        _autoStopTimer = new Timer(
            AutoStopTripCallback,
            null,
            TimeSpan.FromHours(24),
            Timeout.InfiniteTimeSpan
        );

        _uiUpdateTimer = new Timer(
            UiUpdateTimerCallback,
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1)
        );

        _isRunning = true;
        return true;
    }

    public async Task StopTrackingAsync()
    {
        if (!_isRunning) return;

        _uiUpdateTimer?.Dispose();
        _uiUpdateTimer = null;

        StopPedometer();
        StopGeolocation();
        _autoStopTimer?.Dispose();

        _currentTrip.Duration = (DateTime.UtcNow - _startTime).TotalSeconds;
        _currentTrip.Distance = _totalDistanceKm;
        _currentTrip.Steps = _currentSteps;

        await _tripRepository.SaveTripAsync(_currentTrip, new List<LocalGeoPoint>());

        _notificationService.StopForegroundService();

        _isRunning = false;
        _currentTrip = null;

        await _syncService.SynchronizePendingTripsAsync();
    }

    private void StartPedometer()
    {
        _pedometerService.Stop();
        _pedometerService.ReadingChanged += OnPedometerChanged;
        _pedometerService.Start();
    }

    private void StopPedometer()
    {
        _pedometerService.ReadingChanged -= OnPedometerChanged;
        _pedometerService.Stop();
    }

    private async Task StartGeolocation()
    {
        try
        {
            Geolocation.Default.LocationChanged += OnLocationChanged;
            var request = new GeolocationListeningRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5));
            await Geolocation.Default.StartListeningForegroundAsync(request);
        }
        catch (Exception ex)
        {
            throw new Exception("Błąd podczas uruchamiania geolokalizacji: " + ex.Message);
        }
    }

    private void StopGeolocation()
    {
        Geolocation.Default.StopListeningForeground();
        Geolocation.Default.LocationChanged -= OnLocationChanged;
    }

    private void OnPedometerChanged(object sender, long steps)
    {
        _currentSteps = steps;
        UpdateStatsAndNotification();
    }

    private void OnLocationChanged(object sender, GeolocationLocationChangedEventArgs e)
    {
        var newLocation = e.Location;
        if (newLocation == null) return;

        double altitude = newLocation.Altitude.HasValue ? newLocation.Altitude.Value : 0.0;

        if (_lastSavedLocation == null)
        {
            _lastSavedLocation = newLocation;
            SaveNewGeoPointAsync(newLocation.Latitude, newLocation.Longitude, altitude, newLocation.Timestamp.DateTime);
            return;
        }

        double distanceMeters = Location.CalculateDistance(
            _lastSavedLocation,
            newLocation,
            DistanceUnits.Kilometers) * 1000.0;


        if (distanceMeters >= MinDistanceThresholdMeters)
        {
            _totalDistanceKm += (distanceMeters / 1000.0);

            if (newLocation.Altitude.HasValue && _lastSavedLocation.Altitude.HasValue)
            {
                double altDiff = newLocation.Altitude.Value - _lastSavedLocation.Altitude.Value;
                if (altDiff > 0)
                {
                    _currentTrip.ElevationGain += altDiff;
                }
            }

            _lastSavedLocation = newLocation;
            SaveNewGeoPointAsync(newLocation.Latitude, newLocation.Longitude, altitude, newLocation.Timestamp.DateTime);
        }

        UpdateStatsAndNotification();
    }


    private async void SaveNewGeoPointAsync(double lat, double lon, double alt, DateTime time)
    {
        var newPoint = new LocalGeoPoint
        {
            LocalTripId = _currentTrip.LocalId,
            Latitude = lat,
            Longitude = lon,
            Height = alt,
            Timestamp = time
        };
        await _tripRepository.AddGeoPointAsync(newPoint);
        OnNewGeoPointAdded?.Invoke(new Location(lat, lon));
    }


    private void UpdateStatsAndNotification()
    {
        var duration = DateTime.UtcNow - _startTime;

        var data = new TrackingData
        {
            DistanceKm = _totalDistanceKm,
            Steps = _currentSteps,
            Duration = duration
        };

        OnTripDataUpdated?.Invoke(data);

        string title = $"Dystans: {_totalDistanceKm:F2} km";
        string text = $"Czas: {duration:hh\\:mm\\:ss} | Kroki: {_currentSteps}";
        _notificationService.UpdateNotification(title, text);
    }


    private void AutoStopTripCallback(object state)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await StopTrackingAsync();
        });
    }

    private async Task<bool> CheckPermissionsAsync()
    {
        try
        {
            var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            if (locationStatus != PermissionStatus.Granted)
                return false;

            var activityStatus = await Permissions.CheckStatusAsync<ActivityRecognitionPermission>();
            if (activityStatus != PermissionStatus.Granted)
            {
                activityStatus = await Permissions.RequestAsync<ActivityRecognitionPermission>();
            }
            if (activityStatus != PermissionStatus.Granted)
                return false;

            if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 13)
            {
                var notificationStatus = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                if (notificationStatus != PermissionStatus.Granted)
                {
                    notificationStatus = await Permissions.RequestAsync<Permissions.PostNotifications>();
                }

                if (notificationStatus != PermissionStatus.Granted)
                    return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd podczas sprawdzania uprawnień: {ex.Message}");
            return false;
        }
    }


    private void UiUpdateTimerCallback(object state)
    {
        if (!_isRunning)
        {
            _uiUpdateTimer?.Dispose();
            _uiUpdateTimer = null;
            return;
        }

        UpdateStatsAndNotification();
    }

    public class TrackingData
    {
        public double DistanceKm { get; set; }
        public long Steps { get; set; }
        public TimeSpan Duration { get; set; }
    }
}