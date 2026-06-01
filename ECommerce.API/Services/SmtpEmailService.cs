using ECommerce.API.Options;
using ECommerce.Application.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ECommerce.API.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly IWebHostEnvironment _environment;

    public SmtpEmailService(
        IOptions<EmailOptions> options,
        ILogger<SmtpEmailService> logger,
        IWebHostEnvironment environment)
    {
        _options = options.Value;
        _logger = logger;
        _environment = environment;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.SmtpHost))
        {
            _logger.LogWarning(
                "Email not sent (SMTP disabled or not configured). To: {Email}, Subject: {Subject}. Content: {Content}",
                toEmail,
                subject,
                htmlBody);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        var socketOptions = _options.SmtpPort == 465
            ? SecureSocketOptions.SslOnConnect
            : (_options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, socketOptions, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.SmtpUser))
                await client.AuthenticateAsync(_options.SmtpUser, _options.SmtpPassword, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SMTP failed. Host={Host}, Port={Port}, User={User}. Personal @outlook.com accounts often need smtp-mail.outlook.com instead of smtp.office365.com.",
                _options.SmtpHost,
                _options.SmtpPort,
                _options.SmtpUser);
            throw;
        }
    }
}
