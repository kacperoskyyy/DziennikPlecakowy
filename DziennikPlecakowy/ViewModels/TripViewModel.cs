using System.Windows.Input;
using DziennikPlecakowy.Interfaces;
using Microsoft.Maui.Controls;

namespace DziennikPlecakowy.ViewModels
{
    public class TripViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private bool _isTracking;
        public string Status { get; set; } = "Nie śledzisz";
        public string StartButtonText => _isTracking ? "Zakończ" : "Start";

        public ICommand ToggleCommand { get; }

        public TripViewModel(ITripService tripService)
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
                await _tripService.StopAndSaveAsync("dummy");
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
