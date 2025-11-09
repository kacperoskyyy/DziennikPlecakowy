using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models;
using DziennikPlecakowy.Services.Local;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace DziennikPlecakowy.ViewModels;

public partial class AdminViewModel : BaseViewModel
{
    private readonly ApiClientService _apiClient;
    public IAsyncRelayCommand GoBackAsyncCommand { get; }

    [ObservableProperty]
    ObservableCollection<AdminUserDetailDTO> users;

    private List<AdminUserDetailDTO> allUsers = new List<AdminUserDetailDTO>();

    [ObservableProperty]
    string searchText;

    public AdminViewModel(ApiClientService apiClient)
    {
        _apiClient = apiClient;
        Title = "Panel Administratora";
        users = new ObservableCollection<AdminUserDetailDTO>();
        GoBackAsyncCommand = new AsyncRelayCommand(GoBackAsync);
    }

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var response = await _apiClient.GetAsync("/api/Admin/getAllUsers");
            if (response.IsSuccessStatusCode)
            {
                allUsers = await response.Content.ReadFromJsonAsync<List<AdminUserDetailDTO>>();
                FilterUsers();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterUsers();
    }

    private void FilterUsers()
    {
        users.Clear();
        IEnumerable<AdminUserDetailDTO> filtered;

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = allUsers;
        }
        else
        {
            filtered = allUsers.Where(u =>
                u.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                u.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            );
        }

        foreach (var user in filtered)
        {
            users.Add(user);
        }
    }

    [RelayCommand]
    private async Task ShowUserActionsAsync(AdminUserDetailDTO user)
    {
        if (user == null || IsBusy) return;

        string blockAction = user.IsLocked ? "Odblokuj użytkownika" : "Zablokuj użytkownika";
        string roleAction = user.Roles.Contains(UserRole.Admin) ? "Odbierz Admina" : "Nadaj Admina";

        string action = await Shell.Current.DisplayActionSheet(
            $"Akcje dla: {user.Username}",
            "Anuluj",
            "Usuń użytkownika",
            blockAction,
            "Wymuś zmianę hasła",
            roleAction);

        if (string.IsNullOrEmpty(action) || action == "Anuluj")
            return;

        IsBusy = true;
        try
        {
            HttpResponseMessage response = null;

            if (action == "Usuń użytkownika")
            {
                bool confirmed = await Shell.Current.DisplayAlert(
                    "Potwierdź usunięcie",
                    $"Czy na pewno chcesz nieodwracalnie usunąć użytkownika {user.Username}?",
                    "Tak, usuń",
                    "Anuluj"
                );

                if (!confirmed)
                {
                    IsBusy = false;
                    return;
                }
                response = await _apiClient.DeleteAsync($"/api/Admin/deleteUser/{user.Id}");
            }
            else if (action == "Zablokuj użytkownika")
            {
                response = await _apiClient.PutAsync($"/api/Admin/blockUser/{user.Id}", null);
            }
            else if (action == "Odblokuj użytkownika")
            {
                response = await _apiClient.PutAsync($"/api/Admin/unblockUser/{user.Id}", null);
            }
            else if (action == "Wymuś zmianę hasła")
            {
                response = await _apiClient.PutAsync($"/api/Admin/forcePasswordChange/{user.Id}", null);
            }
            else if (action == "Nadaj Admina")
            {
                var req = new AdminChangeRoleRequestDTO { UserId = user.Id, NewRole = UserRole.Admin };
                response = await _apiClient.PostAsJsonAsync("/api/Admin/changeRole", req);
            }
            else if (action == "Odbierz Admina")
            {
                var req = new AdminChangeRoleRequestDTO { UserId = user.Id, NewRole = UserRole.User };
                response = await _apiClient.PostAsJsonAsync("/api/Admin/changeRole", req);
            }

            if (response != null && !response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("Błąd", "Operacja nie powiodła się.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Błąd krytyczny", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            await LoadUsersAsync();
        }
    }
    private async Task GoBackAsync()
    {
        if (IsBusy) return;

        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd nawigacji: {ex.Message}");
        }
    }
}