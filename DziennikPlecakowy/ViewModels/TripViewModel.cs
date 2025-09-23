using System.Windows.Input;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Services;
using Microsoft.Maui.Controls;

namespace DziennikPlecakowy.ViewModels
{
    public class TripViewModel : BaseViewModel
    {
        private readonly TripServiceClient _tripService;
        private bool _isTracking;
        public string Status { get; set; } = "Nie śledzisz";
        public string StartButtonText => _isTracking ? "Zakończ" : "Start";

        public ICommand ToggleCommand { get; }

        public TripViewModel(TripServiceClient tripService)
        {
            _tripService = tripService;
            ToggleCommand = new Command(async () => await ToggleAsync());
        }

        private async Task ToggleAsync()
        {
            if (_isTracking)
            {
                _isTracking = false;
                Status = "Zakończono";
                await _tripService.StopAndSaveAsync(SecureStorage.Default.GetAsync("userId").Result);
            }
            else
            {
                _isTracking = true;
                Status = "Trwa śledzenie...";
                _tripService.StartTracking();
            }
            OnPropertyChanged(nameof(StartButtonText));
            OnPropertyChanged(nameof(Status));
        }
    }
}
