using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Services.Local;
using DziennikPlecakowy.Views;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DziennikPlecakowy.ViewModels;

public partial class EditAccountViewModel : BaseViewModel
{
    private readonly ApiClientService _apiClient;
    private readonly AuthService _authService;

    [ObservableProperty] string newUsername;
    [ObservableProperty] string currentPassword;
    [ObservableProperty] string newPassword;
    [ObservableProperty] string confirmNewPassword;

    [ObservableProperty] string usernameMessage;
    [ObservableProperty] string emailMessage;
    [ObservableProperty] string passwordMessage;
    [ObservableProperty] string deleteMessage;


    private const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$";
    private const string UsernameRegex = @"^[a-zA-Z0-9_]+$";

    public EditAccountViewModel(ApiClientService apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
        Title = "Edytuj konto";
    }

    [RelayCommand]
    private async Task ChangeUsernameAsync()
    {
        var cleanNewUsername = NewUsername?.Trim();

        if (string.IsNullOrWhiteSpace(cleanNewUsername))
        {
            UsernameMessage = "Nazwa użytkownika nie może być pusta.";
            return;
        }

        if (cleanNewUsername.Length > 30)
        {
            UsernameMessage = "Nazwa użytkownika nie może przekraczać 30 znaków.";
            return;
        }

        if (!Regex.IsMatch(cleanNewUsername, UsernameRegex))
        {
            UsernameMessage = "Nazwa użytkownika może zawierać tylko litery (a-z), cyfry (0-9) i podkreślenia (_).";
            return;
        }

        if (IsBusy) return;
        IsBusy = true;
        UsernameMessage = string.Empty;

        var request = new UserChangeNameRequestDTO { NewUsername = cleanNewUsername };
        var response = await _apiClient.PutAsync("/api/User/changeName", JsonContent.Create(request));

        if (response.IsSuccessStatusCode)
        {
            UsernameMessage = "Nazwa użytkownika została zmieniona.";
            _authService.SetCurrentUserProfile(null);
        }
        else
        {
            UsernameMessage = "Wystąpił błąd podczas zmiany nazwy użytkownika.";
        }
        IsBusy = false;
    }


    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        var cleanCurrentPassword = CurrentPassword;
        var cleanNewPassword = NewPassword;
        var cleanConfirmNewPassword = ConfirmNewPassword;

        if (string.IsNullOrWhiteSpace(cleanCurrentPassword) || string.IsNullOrWhiteSpace(cleanNewPassword))
        {
            PasswordMessage = "Wszystkie pola hasła są wymagane.";
            return;
        }
        if (cleanNewPassword != cleanConfirmNewPassword)
        {
            PasswordMessage = "Nowe hasła nie są takie same.";
            return;
        }

        if (cleanNewPassword.Length > 50)
        {
            PasswordMessage = "Nowe hasło nie może przekraczać 50 znaków.";
            return;
        }

        if (cleanCurrentPassword.Length > 50)
        {
            PasswordMessage = "Bieżące hasło jest nieprawidłowe.";
            return;
        }

        if (!Regex.IsMatch(cleanNewPassword, PasswordRegex))
        {
            PasswordMessage = "Nowe hasło musi mieć min. 6 znaków, 1 dużą literę, 1 cyfrę i 1 znak specjalny.";
            return;
        }

        if (IsBusy) return;
        IsBusy = true;
        PasswordMessage = string.Empty;

        try
        {
            var request = new UserChangePasswordRequestDTO
            {
                Password = cleanCurrentPassword,
                NewPassword = cleanNewPassword
            };
            var response = await _apiClient.PutAsJsonAsync("/api/User/changePassword", request);

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
                string errorMsg = "Błąd zmiany hasła. Sprawdź bieżące hasło lub czy nowe hasło nie jest takie samo jak stare.";
                try
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(jsonResponse))
                    {
                        var errorDto = System.Text.Json.JsonSerializer.Deserialize<ErrorResponseDTO>(jsonResponse);
                        if (!string.IsNullOrEmpty(errorDto?.Message))
                        {
                            errorMsg = errorDto.Message;
                        }
                    }
                }
                catch (JsonException)
                {
                }
                PasswordMessage = errorMsg;
            }
        }
        catch (Exception ex)
        {
            PasswordMessage = $"Wyjątek: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAccountAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        DeleteMessage = string.Empty;

        try
        {
            string error = await _authService.RequestAccountDeletionAsync();

            if (error == null)
            {
                await Shell.Current.GoToAsync(nameof(ConfirmDeletionPage));
            }
            else
            {
                DeleteMessage = error;
            }
        }
        catch (Exception ex)
        {
            DeleteMessage = $"Wystąpił wyjątek: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}