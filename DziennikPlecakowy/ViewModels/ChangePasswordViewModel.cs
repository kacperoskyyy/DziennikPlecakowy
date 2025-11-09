using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;
using System.Net.Http.Json;

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

    public ChangePasswordViewModel(ApiClientService apiClient, SyncService syncService)
    {
        _apiClient = apiClient;
        _syncService = syncService;
        Title = "Zmień hasło";
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmNewPassword))
        {
            ErrorMessage = "Wszystkie pola są wymagane.";
            return;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            ErrorMessage = "Hasła nie są zgodne.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new UserChangePasswordRequestDTO
            {
                Password = "",
                NewPassword = NewPassword
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