using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;
using System.Text.RegularExpressions;

namespace DziennikPlecakowy.ViewModels;

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
    private const string EmailRegex = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
    private const string UsernameRegex = @"^[a-zA-Z0-9_]+$";

    public RegisterViewModel(AuthService authService, SyncService syncService)
    {
        _authService = authService;
        _syncService = syncService;
        Title = "Rejestracja";
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        var cleanUsername = Username?.Trim();
        var cleanEmail = Email?.Trim();
        var cleanPassword = Password;
        var cleanConfirmPassword = ConfirmPassword;

        if (string.IsNullOrWhiteSpace(cleanUsername) ||
            string.IsNullOrWhiteSpace(cleanEmail) ||
            string.IsNullOrWhiteSpace(cleanPassword))
        {
            ErrorMessage = "Wszystkie pola są wymagane.";
            return;
        }

        if (cleanUsername.Length > 30)
        {
            ErrorMessage = "Nazwa użytkownika nie może przekraczać 30 znaków.";
            return;
        }
        if (cleanEmail.Length > 100)
        {
            ErrorMessage = "E-mail nie może przekraczać 100 znaków.";
            return;
        }
        if (cleanPassword.Length > 50)
        {
            ErrorMessage = "Hasło nie może przekraczać 50 znaków.";
            return;
        }

        if (!Regex.IsMatch(cleanUsername, UsernameRegex))
        {
            ErrorMessage = "Nazwa użytkownika może zawierać tylko litery (a-z), cyfry (0-9) i podkreślenia (_).";
            return;
        }

        if (!Regex.IsMatch(cleanEmail, EmailRegex))
        {
            ErrorMessage = "Wprowadź poprawny adres e-mail.";
            return;
        }

        if (cleanPassword != cleanConfirmPassword)
        {
            ErrorMessage = "Hasła nie są takie same.";
            return;
        }

        if (IsBusy) return;

        if (!Regex.IsMatch(cleanPassword, PasswordRegex))
        {
            ErrorMessage = "Hasło musi mieć min. 6 znaków, 1 dużą literę, 1 cyfrę i 1 znak specjalny.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var result = await _authService.RegisterAsync(cleanUsername, cleanEmail, cleanPassword);

            if (result.IsSuccess)
            {
                await Shell.Current.DisplayAlert("Sukces", "Konto utworzone.", "OK");
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