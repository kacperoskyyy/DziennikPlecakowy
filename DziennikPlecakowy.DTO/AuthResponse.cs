namespace DziennikPlecakowy.DTO
{
    // DTO dla odpowiedzi uwierzytelniania z tokenem JWT i tokenem odświeżającym
    public class AuthResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}