using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;

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