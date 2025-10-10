using System.Windows.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using Microsoft.Maui.Controls;

namespace DziennikPlecakowy.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService _auth;
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public ICommand RegisterCommand { get; }
        public RegisterViewModel(IAuthService auth)
        {
            _auth = auth;
            RegisterCommand = new Command(async () => await RegisterAsync());
        }

        private async Task RegisterAsync()
        {
            var req = new UserRegisterRequest { Email = Email, Username = Username, Password = Password };
            var ok = await _auth.RegisterAsync(req);
            if (ok) await Application.Current.MainPage.DisplayAlert("OK", "Zarejestrowano", "OK");
            else await Application.Current.MainPage.DisplayAlert("Błąd", "Rejestracja nie powiodła się, spróbuj ponownie później.", "OK");
        }
    }
}
