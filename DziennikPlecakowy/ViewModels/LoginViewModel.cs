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
        public bool IsErrorVisible { get; set; }

        public LoginViewModel(AuthServiceClient auth)
        {
            _auth = auth;
            LoginCommand = new Command(async () => await LoginAsync());
            GoRegisterCommand = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new Views.RegisterPage()));
            IsErrorVisible = false;
        }

        private async Task LoginAsync()
        {
            var req = new UserAuthRequest { Email = Email, Password = Password };
            var res = await _auth.LoginAsync(req);
            if (res != null)
            {
                await SecureStorage.Default.SetAsync("auth_token",  res);
                Application.Current.MainPage = new NavigationPage(new Views.TripPage());
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", "Niepoprawne dane", "OK");
            }
        }
    }
}
