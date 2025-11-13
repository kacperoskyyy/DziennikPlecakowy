using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models;
using DziennikPlecakowy.Repositories;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;
using System.Net.Http.Json;

namespace DziennikPlecakowy.ViewModels;

// ViewModel zarządzający ekranem konta użytkownika

public partial class AccountViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly ApiClientService _apiClientService;
    private readonly LocalTripRepository _localTripRepository;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAdmin))]
    UserProfileDTO userProfile;

    public bool IsAdmin => UserProfile?.Roles?.Contains(UserRole.Admin) ?? false;
    public AccountViewModel(AuthService authService, ApiClientService apiClientService, LocalTripRepository localTripRepository)
    {
        _authService = authService;
        _apiClientService = apiClientService;
        _localTripRepository = localTripRepository;
        Title = "Moje konto";
    }

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        UserProfile = null;

        try
        {
                var response = await _apiClientService.GetAsync("/api/User/getUserStats");
                if (response.IsSuccessStatusCode)
                {
                    UserProfile = await response.Content.ReadFromJsonAsync<UserProfileDTO>();
                    _authService.SetCurrentUserProfile(UserProfile);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Nie udało się załadować profilu użytkownika: {response}");
                }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Wystąpił błąd podczas ładowania profilu użytkownika: {ex.Message}");
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

        UserProfile = null;
        await _localTripRepository.DeletaAllTrips();

        await Shell.Current.GoToAsync(nameof(LoginPage));
    }
    [RelayCommand]
    private async Task GoToAdminPanelAsync()
    {
         await Shell.Current.GoToAsync(nameof(AdminPage));
    }
}
