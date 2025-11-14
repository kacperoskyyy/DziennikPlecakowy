using DziennikPlecakowy.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DziennikPlecakowy.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetCode)
    {
        var smtpHost = _config["SmtpSettings:Host"];
        var smtpPortString = _config["SmtpSettings:Port"];
        var smtpPort = int.TryParse(smtpPortString, out var p) ? p : 0;
        var fromEmail = _config["SmtpSettings:FromEmail"];
        var fromName = _config["SmtpSettings:FromName"] ?? "Dziennik Plecakowy";
        var appPassword = _config["SmtpSettings:AppPassword"];

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(appPassword) || smtpPort == 0)
        {
            _logger.LogError("Ustawienia SMTP (SmtpSettings) nie są poprawnie skonfigurowane w appsettings.json.");
            return;
        }

   
        var subject = "Dziennik Plecakowy - Resetowanie Hasła";
        var body = $"Witaj,\n\nOtrzymaliśmy prośbę o zresetowanie hasła do Twojego konta.\n\nTwój kod do resetowania hasła to: {resetCode}\n\nKod jest ważny przez 15 minut.\n\nJeśli to nie Ty prosiłeś o reset, zignoruj tę wiadomość.\n\nPozdrawiamy,\nZespół Dziennik Plecakowy";

   
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using (var client = new SmtpClient())
        {
            try
            {
                _logger.LogInformation("Łączenie z serwerem SMTP: {Host}:{Port}...", smtpHost, smtpPort);


                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);

                _logger.LogInformation("Uwierzytelnianie na serwerze SMTP...");
                await client.AuthenticateAsync(fromEmail, appPassword);

                _logger.LogInformation("Wysyłanie e-maila resetującego hasło do {Email}", toEmail);
                await client.SendAsync(message);

                await client.DisconnectAsync(true);
                _logger.LogInformation("E-mail wysłany pomyślnie do {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas wysyłania e-maila (MailKit) do {Email}", toEmail);
            }
        }
    }
}