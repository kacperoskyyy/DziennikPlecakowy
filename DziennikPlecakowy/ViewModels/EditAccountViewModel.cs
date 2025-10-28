using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Services.Local;
using System.Net.Http.Json;

namespace DziennikPlecakowy.ViewModels;
//View Model do edycji konta użytkownika

public partial class EditAccountViewModel : BaseViewModel
{
    private readonly ApiClientService _apiClient;
    private readonly AuthService _authService;

    [ObservableProperty] string newUsername;
    [ObservableProperty] string newEmail;
    [ObservableProperty] string currentPassword;
    [ObservableProperty] string newPassword;
    [ObservableProperty] string confirmNewPassword;

    [ObservableProperty] string usernameMessage;
    [ObservableProperty] string emailMessage;
    [ObservableProperty] string passwordMessage;

    public EditAccountViewModel(ApiClientService apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
        Title = "Edytuj konto";
    }

    [RelayCommand]
    private async Task ChangeUsernameAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUsername))
        {
            UsernameMessage = "Nazwa użytkownika nie może być pusta.";
            return;
        }
        if(IsBusy) return;
        IsBusy = true;

        var request = new UserChangeNameRequestDTO { NewUsername = NewUsername };
        var response = await _apiClient.PostAsJsonAsync("/api/User.changeName", request);

        if(response.IsSuccessStatusCode)
        {
            UsernameMessage = "Nazwa użytkownika została zmieniona.";
        }
        else
        {
            UsernameMessage = "Wystąpił błąd podczas zmiany nazwy użytkownika.";
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task ChangeEmailAsync()
    {
        if (string.IsNullOrWhiteSpace(NewEmail))
        {
            EmailMessage = "Email nie może być pusty.";
            return;
        }
        if (IsBusy) return;
        IsBusy = true;

        var request = new UserChangeEmailRequestDTO { NewEmail = NewEmail };
        var response = await _apiClient.PostAsJsonAsync("/api/User/changeEmail", request);

        if (response.IsSuccessStatusCode)
        {
            EmailMessage = "Email zmieniony pomyślnie!";
        }
        else
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponseDTO>();
            EmailMessage = error?.Message ?? "Błąd zmiany emaila (może być zajęty).";
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            PasswordMessage = "Wszystkie pola hasła są wymagane.";
            return;
        }
        if (NewPassword != ConfirmNewPassword)
        {
            PasswordMessage = "Nowe hasła nie są takie same.";
            return;
        }
        if (IsBusy) return;
        IsBusy = true;

        var request = new UserChangePasswordRequestDTO
        {
            Password = CurrentPassword,
            NewPassword = NewPassword
        };
        var response = await _apiClient.PostAsJsonAsync("/api/User/changePassword", request);

        if (response.IsSuccessStatusCode)
        {
            PasswordMessage = "Hasło zmienione pomyślnie!";
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
            OnPropertyChanged(nameof(CurrentPassword));
            OnPropertyChanged(nameof(NewPassword));
            OnPropertyChanged(nameof(ConfirmNewPassword));
        }
        else
        {

            var error = await response.Content.ReadFromJsonAsync<ErrorResponseDTO>();
            PasswordMessage = error?.Message ?? "Błąd zmiany hasła.";
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

}
