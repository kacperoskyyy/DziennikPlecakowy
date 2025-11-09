using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;
using System.Text.RegularExpressions;

namespace DziennikPlecakowy.ViewModels;

// View Model do rejestracji użytkowników
public partial class RegisterViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly SyncService _syncService;

    [ObservableProperty]
    string username;

    [ObservableProperty]
    string email;

    [ObservableProperty]
    string password;

    [ObservableProperty]
    string confirmPassword;

    [ObservableProperty]
    string errorMessage;

    private const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$";

    public RegisterViewModel(AuthService authService, SyncService syncService)
    {
        _authService = authService;
        _syncService = syncService;
        Title = "Rejestracja";
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Wszystkie pola są wymagane.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Hasła nie są takie same.";
            return;
        }

        if (IsBusy) return;

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Hasła nie są zgodne.";
            return;
        }

        if (!Regex.IsMatch(Password, PasswordRegex))
        {
            ErrorMessage = "Hasło musi mieć min. 6 znaków, 1 dużą literę, 1 cyfrę i 1 znak specjalny.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var result = await _authService.RegisterAsync(Username, Email, Password);

            if (result.IsSuccess)
            {
                await Shell.Current.DisplayAlert("Sukces", "Konto utworzone. Możesz się teraz zalogować.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Wystąpił nieoczekiwany błąd: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}