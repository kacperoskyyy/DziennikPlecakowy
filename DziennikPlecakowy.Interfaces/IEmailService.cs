namespace DziennikPlecakowy.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetCode);
    Task SendAccountDeletionEmailAsync(string toEmail, string deletionCode);
}