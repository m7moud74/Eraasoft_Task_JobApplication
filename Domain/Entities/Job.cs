using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities;

public class Job
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? CreatedByUserId { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public int? RecruiterId { get; set; }
    public Recruiter? Recruiter { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

