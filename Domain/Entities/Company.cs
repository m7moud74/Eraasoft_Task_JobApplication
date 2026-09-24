using System;
using System.Collections.Generic;
using JobApplication.Domain.Enums;

namespace JobApplication.Domain.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public CompanyStatus Status { get; set; } = CompanyStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByUserId { get; set; }

    public ICollection<Recruiter> Recruiters { get; set; } = new List<Recruiter>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();

    public void Approve(string adminUserId)
    {
        Status = CompanyStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByUserId = adminUserId;
    }

    public void Reject(string adminUserId)
    {
        Status = CompanyStatus.Rejected;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByUserId = adminUserId;
    }
}
