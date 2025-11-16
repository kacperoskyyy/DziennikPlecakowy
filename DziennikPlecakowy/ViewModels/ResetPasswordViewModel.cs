using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;

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

    public ResetPasswordViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Resetowanie hasła";
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        if (IsBusy) return;

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Token) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "Wszystkie pola są wymagane.";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Hasła nie są identyczne.";
            return;
        }

        if (NewPassword.Length < 6)
        {
            ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.";
            return;
        }

        try
        {
            IsBusy = true;

            string error = await _authService.ResetPasswordAsync(Token, NewPassword);

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