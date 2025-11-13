using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;

namespace DziennikPlecakowy.ViewModels;
// View Model do logowania użytkownika

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

    public LoginViewModel(AuthService authService, SyncService syncService)
    {
        _authService = authService;
        _syncService = syncService;
        Title = "Logowanie";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email i hasło są wymagane.";
            return;
        }

        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var result = await _authService.LoginAsync(Email, Password);

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
}