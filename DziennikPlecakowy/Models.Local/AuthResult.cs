namespace DziennikPlecakowy.Models.Local;

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public bool MustChangePassword { get; set; }

    public static AuthResult Success(bool mustChange = false) =>
        new() { IsSuccess = true, MustChangePassword = mustChange };

    public static AuthResult Fail(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}