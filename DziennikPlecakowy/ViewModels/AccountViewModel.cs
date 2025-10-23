using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;
using System.Net.Http.Json;

namespace DziennikPlecakowy.ViewModels;

public partial class AccountViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly ApiClientService _apiClientService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAdmin))]
    UserProfileDTO userProfile;

    public bool IsAdmin => UserProfile?.Roles?.Contains("Admin") ?? false;
    public AccountViewModel(AuthService authService, ApiClientService apiClientService)
    {
        _authService = authService;
        _apiClientService = apiClientService;
        Title = "Moje konto";
    }

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var response = await _apiClientService.GetAsync("/api/User/getUserStats");
            if (response.IsSuccessStatusCode)
            {
                UserProfile = await response.Content.ReadFromJsonAsync<UserProfileDTO>();
            }
            else
            {
                throw new Exception($"Nie udało się załadować profilu użytkownika: {response}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Wystąpił błąd podczas ładowania profilu użytkownika.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToEditAccountAsync()
    {
        await Shell.Current.GoToAsync(nameof(EditAccountPage));
    }
    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }
    [RelayCommand]
    private async Task GoToAdminPanelAsync()
    {
        // await Shell.Current.GoToAsync(nameof(AdminPage));
    }
}
