using System.Collections.ObjectModel;
using System.Windows.Input;
using DziennikPlecakowy.Models;
using DziennikPlecakowy.Interfaces;
using Microsoft.Maui.Controls;

namespace DziennikPlecakowy.ViewModels
{
    public class TripsListViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        public ObservableCollection<Trip> Trips { get; set; } = new();
        public ICommand LoadCommand { get; }

        public TripsListViewModel(ITripService tripService)
        {
            _tripService = tripService;
            LoadCommand = new Command(async () => await LoadAsync());
        }

        private async Task LoadAsync()
        {
            var userId = SecureStorage.Default.GetAsync("userId").Result;
            var list = await _tripService.GetUserTripsAsync(userId);
            Trips.Clear();
            foreach (var t in list) Trips.Add((Trip)t);
        }
    }
}
