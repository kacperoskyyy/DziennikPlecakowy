using System.Windows.Input;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.DTO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using DziennikPlecakowy.Services;

namespace DziennikPlecakowy.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly  AuthServiceClient _auth;
        public string Email { get; set; }
        public string Password { get; set; }

        public ICommand LoginCommand { get; }
        public ICommand GoRegisterCommand { get; }

        public LoginViewModel(IAuthService auth)
        {
            _auth = auth;
            LoginCommand = new Command(async () => await LoginAsync());
            GoRegisterCommand = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new Views.RegisterPage()));
        }

        private async Task LoginAsync()
        {
            var req = new UserAuthRequest { Email = Email, Password = Password };
            var res = await _auth.LoginAsync(req);
            if (res != null)
            {
                // ustaw token i przejscie do shell
                await SecureStorage.Default.SetAsync("auth_token", res.Token);
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", "Niepoprawne dane", "OK");
            }
        }
    }
}
