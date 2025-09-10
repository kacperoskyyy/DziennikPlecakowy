using System.Windows.Input;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.DTO;
using Microsoft.Maui.Controls;

namespace DziennikPlecakowy.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        public string Username { get; set; }
        public string Email { get; set; }

        public ICommand SaveCommand { get; }

        public ProfileViewModel(IUserService userService)
        {
            _userService = userService;
            SaveCommand = new Command(async () => await SaveAsync());
            // w rzeczywistości załaduj usera z API
        }

        private async Task SaveAsync()
        {
            var req = new UserChangeNameRequest { UserName = Username, Email = Email };
            var ok = await _userService.ChangeNameAsync(req);
            if (ok) await Application.Current.MainPage.DisplayAlert("OK", "Zapisano", "OK");
            else await Application.Current.MainPage.DisplayAlert("Błąd", "Nie udało się", "OK");
        }
    }
}
