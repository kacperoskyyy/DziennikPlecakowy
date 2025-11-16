using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace DziennikPlecakowy.ViewModels;

public partial class ChangePasswordViewModel : BaseViewModel
{
    private readonly ApiClientService _apiClient;
    private readonly SyncService _syncService;

    [ObservableProperty]
    string newPassword;

    [ObservableProperty]
    string confirmNewPassword;

    [ObservableProperty]
    string errorMessage;

    private const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$";

    public ChangePasswordViewModel(ApiClientService apiClient, SyncService syncService)
    {
        _apiClient = apiClient;
        _syncService = syncService;
        Title = "Zmień hasło";
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        var cleanNewPassword = NewPassword;
        var cleanConfirmNewPassword = ConfirmNewPassword;

        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(cleanNewPassword) || string.IsNullOrWhiteSpace(cleanConfirmNewPassword))
        {
            ErrorMessage = "Wszystkie pola są wymagane.";
            return;
        }

        if (cleanNewPassword != cleanConfirmNewPassword)
        {
            ErrorMessage = "Hasła nie są zgodne.";
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

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new UserChangePasswordRequestDTO
            {
                Password = "",
                NewPassword = cleanNewPassword
            };


            var response = await _apiClient.PutAsync(
                "/api/User/changePassword",
                JsonContent.Create(request)
            );

            if (response.IsSuccessStatusCode)
            {
                await _syncService.SynchronizePendingTripsAsync();
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = "Nie udało się zmienić hasła. Sprawdź, czy nowe hasło nie jest takie samo jak stare.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}