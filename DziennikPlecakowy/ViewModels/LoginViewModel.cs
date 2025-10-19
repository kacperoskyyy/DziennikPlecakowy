using System.Windows.Input;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using DziennikPlecakowy.Services;
using ReactiveUI;
using System.Reactive;

namespace DziennikPlecakowy.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly  AuthServiceClient _auth;
        private string _email;
        private string _password;
        public string Email 
        {
            get => _email; 
            set => SetProperty(ref _email, value);
        }
        public string Password
        { 
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ReactiveCommand<Unit,Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> GoRegisterCommand { get; }

        public LoginViewModel(AuthServiceClient auth)
        {
            _auth = auth;
            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
            GoRegisterCommand = ReactiveCommand.CreateFromTask(GoRegisterAsync);
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", "Aby się zalogować wprowadź email oraz hasło", "OK");
            }
            else
            {
                var req = new UserAuthRequestDTO { Email = Email, Password = Password };
                var res = await _auth.LoginAsync(req);
                if (res != null)
                {
                    await SecureStorage.Default.SetAsync("auth_token", res);
                    Application.Current.MainPage = new NavigationPage(new TripPage());
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Błąd", "Uzytkownik nie istnieje lub wprowadzono niepoprawne hasło", "OK");
                }
            }
        }
        private async Task GoRegisterAsync()
        {
            if (Application.Current?.MainPage == null)
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", "Nie można otworzyć strony rejestracji, brak głównej strony aplikacji.", "OK");
                return;
            }
            Application.Current.MainPage = new RegisterPage();
            await Task.CompletedTask;
        }
    }
}
