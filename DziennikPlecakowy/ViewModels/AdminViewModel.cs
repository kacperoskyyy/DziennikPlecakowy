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

    [ObservableProperty]
    ObservableCollection<AdminUserDetailDTO> users;

    // Bindowanie do SelectedItem w CollectionView
    [ObservableProperty]
    AdminUserDetailDTO selectedUser;

    public AdminViewModel(ApiClientService apiClient)
    {
        _apiClient = apiClient;
        Title = "Panel Administratora";
        users = new ObservableCollection<AdminUserDetailDTO>();
    }

    // Komenda odpalana, gdy strona się pojawia
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
                var userList = await response.Content.ReadFromJsonAsync<List<AdminUserDetailDTO>>();
                users.Clear();
                foreach (var user in userList)
                {
                    users.Add(user);
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Ta metoda odpali się, gdy użytkownik kliknie na kogoś z listy
    partial void OnSelectedUserChanged(AdminUserDetailDTO value)
    {
        if (value == null)
            return;

        // Odpalamy logikę akcji i natychmiast czyścimy zaznaczenie
        ShowUserActionsCommand.Execute(value);
        SelectedUser = null; // Resetuj zaznaczenie
    }

    [RelayCommand]
    private async Task ShowUserActionsAsync(AdminUserDetailDTO user)
    {
        if (user == null || IsBusy) return;

        // Definiujemy akcje na podstawie stanu usera
        string blockAction = user.IsLocked ? "Odblokuj użytkownika" : "Zablokuj użytkownika";
        string roleAction = user.Roles.Contains(UserRole.Admin) ? "Odbierz Admina" : "Nadaj Admina";

        string action = await Shell.Current.DisplayActionSheet(
            $"Akcje dla: {user.Username}",
            "Anuluj",
            null,
            blockAction,
            "Wymuś zmianę hasła",
            roleAction);

        if (string.IsNullOrEmpty(action) || action == "Anuluj")
            return;

        IsBusy = true;
        try
        {
            HttpResponseMessage response = null;

            if (action == "Zablokuj użytkownika")
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
            // Odśwież listę po akcji
            await LoadUsersAsync();
        }
    }
}