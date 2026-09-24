using System;
using System.Threading;
using System.Threading.Tasks;
using JobApplication.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace JobApplication.Infrastructure.Services;

public class MailKitEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<MailKitEmailService> _logger;

    public MailKitEmailService(IOptions<EmailSettings> options, ILogger<MailKitEmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("Recipient email address cannot be empty.", nameof(toEmail));
        }

        var message = new MimeMessage();
        var senderName = string.IsNullOrWhiteSpace(_settings.SenderName) ? "Track Application System" : _settings.SenderName;
        var senderEmail = string.IsNullOrWhiteSpace(_settings.SenderEmail) ? "noreply@trackapplication.com" : _settings.SenderEmail;

        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        // If SMTP server is not configured (e.g. local dev without SMTP), log and return without crashing
        if (string.IsNullOrWhiteSpace(_settings.SmtpServer))
        {
            _logger.LogWarning("SMTP server is not configured in settings. Email to {ToEmail} with subject '{Subject}' was not dispatched over network.", toEmail, subject);
            return;
        }

        using var client = new SmtpClient();
        try
        {
            var secureSocketOptions = _settings.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTlsWhenAvailable;

            await client.ConnectAsync(_settings.SmtpServer, _settings.Port, secureSocketOptions, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_settings.Username) && !string.IsNullOrWhiteSpace(_settings.Password))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            _logger.LogInformation("Email sent successfully to {ToEmail} with subject '{Subject}'.", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail} via SMTP server {Server}:{Port}.", toEmail, _settings.SmtpServer, _settings.Port);
            throw; // Rethrow so Hangfire knows the job failed and can retry
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}
