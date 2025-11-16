using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;
using System.Text.RegularExpressions;

namespace DziennikPlecakowy.ViewModels;

public partial class ForgotPasswordViewModel : BaseViewModel
{

    [ObservableProperty]
    string email;

    [ObservableProperty]
    string errorMessage;

    [ObservableProperty]
    string successMessage;

    private readonly AuthService _authService;

    private const string EmailRegex = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";

    public ForgotPasswordViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Resetowanie hasła";
    }

    [RelayCommand]
    private async Task SendResetLinkAsync()
    {
        var cleanEmail = Email?.Trim();

        if (IsBusy) return;

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(cleanEmail))
        {
            ErrorMessage = "Proszę podać poprawny adres e-mail.";
            return;
        }

        if (cleanEmail.Length > 100)
        {
            ErrorMessage = "E-mail nie może przekraczać 100 znaków.";
            return;
        }

        if (!Regex.IsMatch(cleanEmail, EmailRegex))
        {
            ErrorMessage = "Proszę podać poprawny adres e-mail.";
            return;
        }

        try
        {
            IsBusy = true;

            string error = await _authService.RequestPasswordResetAsync(cleanEmail);

            if (error == null)
            {
                await Shell.Current.GoToAsync(nameof(ResetPasswordPage));
            }
            else
            {
                ErrorMessage = error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Wystąpił błąd: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        if (IsBusy) return;
        await Shell.Current.GoToAsync("..");
    }
}