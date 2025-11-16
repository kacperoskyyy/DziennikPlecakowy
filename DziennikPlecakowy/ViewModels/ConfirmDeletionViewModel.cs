using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;

namespace DziennikPlecakowy.ViewModels;

public partial class ConfirmDeletionViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    [ObservableProperty]
    string token;

    [ObservableProperty]
    string errorMessage;

    public ConfirmDeletionViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Usuwanie konta";
    }

    [RelayCommand]
    private async Task ConfirmDeletionAsync()
    {
        if (IsBusy) return;

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Token))
        {
            ErrorMessage = "Kod jest wymagany.";
            return;
        }

        try
        {
            IsBusy = true;

            string error = await _authService.ConfirmAccountDeletionAsync(Token);

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