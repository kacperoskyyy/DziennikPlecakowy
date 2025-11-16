using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;
using System.Text.RegularExpressions;

namespace DziennikPlecakowy.ViewModels;

public partial class ConfirmDeletionViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    [ObservableProperty]
    string token;

    [ObservableProperty]
    string errorMessage;


    private const string TokenRegex = @"^[0-9]{6}$";

    public ConfirmDeletionViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Usuwanie konta";
    }

    [RelayCommand]
    private async Task ConfirmDeletionAsync()
    {
        var cleanToken = Token?.Trim();

        if (IsBusy) return;

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(cleanToken) || !Regex.IsMatch(cleanToken, TokenRegex))
        {
            ErrorMessage = "Kod jest wymagany i musi składać się z 6 cyfr.";
            return;
        }

        try
        {
            IsBusy = true;

            string error = await _authService.ConfirmAccountDeletionAsync(cleanToken);

            if (error == null)
            {
                await _authService.LogoutAsync();
                await Shell.Current.Navigation.PopToRootAsync();
                await Shell.Current.GoToAsync(nameof(LoginPage));
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
    private async Task GoBackAsync()
    {
        if (IsBusy) return;
        await Shell.Current.Navigation.PopAsync();
    }
}