using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Docker.DotNet.Models;
using DziennikPlecakowy.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Message = Amazon.SimpleEmail.Model.Message;

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
        var accessKey = _config["AWS:SES:AccessKey"];
        var secretKey = _config["AWS:SES:SecretKey"];
        var region = RegionEndpoint.GetBySystemName(_config["AWS:SES:Region"]);
        var fromEmail = _config["AWS:SES:FromEmail"];

        using var client = new AmazonSimpleEmailServiceClient(accessKey, secretKey, region);

        var subject = "Dziennik Plecakowy - Resetowanie Hasła";
        var body = $"Witaj,\n\nOtrzymaliśmy prośbę o zresetowanie hasła do Twojego konta.\n\nTwój kod do resetowania hasła to: {resetCode}\n\nKod jest ważny przez 15 minut.\n\nJeśli to nie Ty prosiłeś o reset, zignoruj tę wiadomość.\n\nPozdrawiamy,\nZespół Dziennik Plecakowy";

        var sendRequest = new SendEmailRequest
        {
            Source = fromEmail,
            Destination = new Destination { ToAddresses = new List<string> { toEmail } },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = new Body { Text = new Content(body) }
            }
        };

        try
        {
            _logger.LogInformation("Wysyłanie e-maila resetującego hasło do {Email}", toEmail);
            await client.SendEmailAsync(sendRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wysyłania e-maila SES do {Email}", toEmail);
        }
    }
}