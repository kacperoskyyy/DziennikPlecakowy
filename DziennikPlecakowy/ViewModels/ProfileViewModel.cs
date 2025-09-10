using System.Windows.Input;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.DTO;
using Microsoft.Maui.Controls;
using DziennikPlecakowy.Services;

namespace DziennikPlecakowy.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly UserServiceClient _userService;
        public string _Username { get; set; }
        public string _UserId { get; set; }

        public ICommand SaveCommand { get; }

        public ProfileViewModel(UserServiceClient userService)
        {
            _userService = userService;
            SaveCommand = new Command(async () => await SaveAsync());
            // w rzeczywistości załaduj usera z API
        }

        private async Task SaveAsync()
        {
            var req = new UserChangeNameRequest { UserId = _UserId, NewUsername=_Username };
            var ok = await _userService.ChangeNameAsync(req);
            if (ok) await Application.Current.MainPage.DisplayAlert("OK", "Zapisano", "OK");
            else await Application.Current.MainPage.DisplayAlert("Błąd", "Nie udało się", "OK");
        }
    }
}
