using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using Microsoft.Maui.Storage;
using System.Timers;
using Microsoft.Maui.Devices.Sensors;

namespace DziennikPlecakowy.Services
{
    public class TripServiceClient
    {
        private const string OfflineKey = "offline_trips";
        private readonly ApiServiceClient _api;
        private readonly List<GeoPoint> _currentPoints = new();
        private System.Timers.Timer _timer;
        private DateTime _startTime;

        public TripServiceClient(ApiServiceClient api)
        {
            _api = api;
            _timer = new System.Timers.Timer(10000); // 10s
            _timer.Elapsed += Timer_Elapsed;
        }

        public void StartTracking()
        {
            _currentPoints.Clear();
            _startTime = DateTime.UtcNow;
            _timer.Start();
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
                if (location != null)
                {
                    _currentPoints.Add(new GeoPoint
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        Height = location.Altitude ?? 0
                    });
                }
            }
            catch
            {
                // ignoruj błędy lokalizacji (brak uprawnień, brak GPS)
            }
        }

        public async Task StopAndSaveAsync(string userId)
        {
            _timer.Stop();
            var trip = new Trip
            {
                UserId = userId,
                TripDate = _startTime,
                Duration = (DateTime.UtcNow - _startTime).TotalSeconds,
                Distance = 0, // opcjonalnie calculate
                GeoPointList = _currentPoints.ToArray()
            };

            try
            {
                await _api.PostAsync<Trip, Trip>("trips", trip);
            }
            catch
            {
                SaveOffline(trip);
            }
        }

        private void SaveOffline(Trip trip)
        {
            List<Trip> list = new();
            if (Preferences.ContainsKey(OfflineKey))
            {
                var txt = Preferences.Get(OfflineKey, "[]");
                list = JsonSerializer.Deserialize<List<Trip>>(txt) ?? new();
            }
            list.Add(trip);
            Preferences.Set(OfflineKey, JsonSerializer.Serialize(list));
        }

        public async Task SyncOfflineAsync()
        {
            if (!Preferences.ContainsKey(OfflineKey)) return;
            var txt = Preferences.Get(OfflineKey, "[]");
            var list = JsonSerializer.Deserialize<List<Trip>>(txt);
            if (list == null || list.Count == 0) return;

            var failed = new List<Trip>();
            foreach (var t in list)
            {
                try
                {
                    await _api.PostAsync<Trip, Trip>("trips", t);
                }
                catch
                {
                    failed.Add(t);
                }
            }
            Preferences.Set(OfflineKey, JsonSerializer.Serialize(failed));
        }

        public async Task<List<Trip>> GetUserTripsAsync(string userId)
        {
            var res = await _api.GetAsync<List<Trip>>($"trips/user/{userId}");
            return res ?? new List<Trip>();
        }
    }
}
