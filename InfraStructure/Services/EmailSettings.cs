namespace JobApplication.Infrastructure.Services;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    private string _smtpServer = string.Empty;
    public string SmtpHost { get; set; } = string.Empty;
    public string SmtpServer
    {
        get => !string.IsNullOrWhiteSpace(_smtpServer) ? _smtpServer : SmtpHost;
        set => _smtpServer = value;
    }
    public int Port { get; set; } = 587;
    public string SenderName { get; set; } = "Track Application System";
    public string SenderEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = false;
}
