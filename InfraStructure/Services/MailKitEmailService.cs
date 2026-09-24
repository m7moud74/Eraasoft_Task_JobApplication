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

        // If SMTP server is not configured or set to a placeholder (e.g. smtp.example.com), simulate sending in dev
        if (string.IsNullOrWhiteSpace(_settings.SmtpServer) ||
            _settings.SmtpServer.Contains("example.com", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("SMTP server is configured as placeholder ({Server}). Email to {ToEmail} with subject '{Subject}' was simulated successfully.", _settings.SmtpServer, toEmail, subject);
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
            _logger.LogWarning(ex, "Could not send email to {ToEmail} via SMTP server {Server}:{Port}. Email was logged for inspection.", toEmail, _settings.SmtpServer, _settings.Port);
            // In development / local testing, do not re-throw network errors to prevent infinite Hangfire retries
            if (ex is System.Net.Sockets.SocketException || ex is MailKit.Net.Smtp.SmtpCommandException)
            {
                return;
            }
            throw;
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
