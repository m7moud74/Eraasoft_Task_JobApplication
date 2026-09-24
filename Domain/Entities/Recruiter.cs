using System;

namespace JobApplication.Domain.Entities;

public class Recruiter
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string? UserId { get; set; }
    public bool IsCompanyOwner { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
