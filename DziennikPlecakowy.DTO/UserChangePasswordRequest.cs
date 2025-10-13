namespace DziennikPlecakowy.DTO
{
    // DTO dla żądania zmiany hasła użytkownika
    public class UserChangePasswordRequest
    {
        public string Password { get; set; }
        public string NewPassword { get; set; }
    }
}
