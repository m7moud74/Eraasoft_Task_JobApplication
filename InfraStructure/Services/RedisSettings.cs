namespace JobApplication.Infrastructure.Services;

public class RedisSettings
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379";
    public int DefaultExpirationMinutes { get; set; } = 5;
    public bool Enabled { get; set; } = true;
}
