using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;
using System.Text.RegularExpressions;

namespace DziennikPlecakowy.ViewModels;

public partial class ResetPasswordViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    [ObservableProperty]
    string token;

    [ObservableProperty]
    string newPassword;

    [ObservableProperty]
    string confirmPassword;

    [ObservableProperty]
    string errorMessage;

    [ObservableProperty]
    string successMessage;

    private const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$";
    private const string TokenRegex = @"^[0-9]{6}$";

    public ResetPasswordViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Resetowanie hasła";
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        var cleanToken = Token?.Trim();
        var cleanNewPassword = NewPassword;
        var cleanConfirmPassword = ConfirmPassword;

        if (IsBusy) return;

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(cleanToken) || string.IsNullOrWhiteSpace(cleanNewPassword) || string.IsNullOrWhiteSpace(cleanConfirmPassword))
        {
            ErrorMessage = "Wszystkie pola są wymagane.";
            return;
        }

        if (!Regex.IsMatch(cleanToken, TokenRegex))
        {
            ErrorMessage = "Kod jest wymagany i musi składać się z 6 cyfr.";
            return;
        }

        if (cleanNewPassword != cleanConfirmPassword)
        {
            ErrorMessage = "Hasła nie są identyczne.";
            return;
        }

        if (cleanNewPassword.Length > 50)
        {
            ErrorMessage = "Hasło nie może przekraczać 50 znaków.";
            return;
        }

        if (!Regex.IsMatch(cleanNewPassword, PasswordRegex))
        {
            ErrorMessage = "Hasło musi mieć min. 6 znaków, 1 dużą literę, 1 cyfrę i 1 znak specjalny.";
            return;
        }

        try
        {
            IsBusy = true;

            string error = await _authService.ResetPasswordAsync(cleanToken, cleanNewPassword);

            if (error == null)
            {
                SuccessMessage = "Hasło zostało pomyślnie zmienione. Wróć  do logowania...";
                await GoToLoginAsync();
            }
            else
            {
                ErrorMessage = error;
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
        if (IsBusy) return;
        try
        {
            await Shell.Current.Navigation.PopAsync(false);
            await Shell.Current.Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd nawigacji pop: {ex.Message}");
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
    }
}