using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;
using System.Text.RegularExpressions;

namespace DziennikPlecakowy.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly SyncService _syncService;

    [ObservableProperty]
    string email;

    [ObservableProperty]
    string password;

    [ObservableProperty]
    string errorMessage;

    [ObservableProperty]
    bool isCheckingAutoLogin;

    private const string EmailRegex = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";

    public LoginViewModel(AuthService authService, SyncService syncService)
    {
        _authService = authService;
        _syncService = syncService;
        Title = "Logowanie";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        var cleanEmail = Email?.Trim();
        var cleanPassword = Password;

        if (string.IsNullOrWhiteSpace(cleanEmail) || string.IsNullOrWhiteSpace(cleanPassword))
        {
            ErrorMessage = "Email i hasło są wymagane.";
            return;
        }

        if (cleanEmail.Length > 100)
        {
            ErrorMessage = "Email lub hasło są nieprawidłowe.";
            return;
        }
        if (cleanPassword.Length > 50)
        {
            ErrorMessage = "Email lub hasło są nieprawidłowe.";
            return;
        }

        if (!Regex.IsMatch(cleanEmail, EmailRegex))
        {
            ErrorMessage = "Wprowadź poprawny adres e-mail.";
            return;
        }

        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var result = await _authService.LoginAsync(cleanEmail, cleanPassword);

            if (result.IsSuccess)
            {
                if (result.MustChangePassword)
                {
                    await Shell.Current.GoToAsync(nameof(ChangePasswordPage));
                }
                else
                {
                    await _syncService.SynchronizePendingTripsAsync();
                    await Shell.Current.GoToAsync("..");
                }
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
    private async Task GoToRegisterAsync()
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    [RelayCommand]
    private async Task CheckAutoLoginAsync()
    {
        if (IsBusy || IsCheckingAutoLogin) return;

        try
        {
            IsCheckingAutoLogin = true;
            IsBusy = true;
            ErrorMessage = string.Empty;

            var authResult = await _authService.CheckAndRefreshTokenOnStartupAsync();

            if (authResult.IsSuccess)
            {
                if (authResult.MustChangePassword)
                {
                    await Shell.Current.GoToAsync(nameof(ChangePasswordPage));
                }
                else
                {
                    await _syncService.SynchronizePendingTripsAsync();
                    await Shell.Current.GoToAsync("..");
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd auto-logowania: {ex.Message}";
        }
        finally
        {
            IsCheckingAutoLogin = false;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToForgotPasswordAsync()
    {
        if (IsBusy) return;
        await Shell.Current.GoToAsync(nameof(ForgotPasswordPage));
    }
}