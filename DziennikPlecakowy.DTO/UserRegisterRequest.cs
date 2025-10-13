namespace DziennikPlecakowy.DTO
{
    // DTO dla żądania rejestracji użytkownika
    public class UserRegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }
}
