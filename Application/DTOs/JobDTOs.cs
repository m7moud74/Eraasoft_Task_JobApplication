namespace JobApplication.Application.DTOs;

public class JobDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? CreatedByUserId { get; set; }
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? RecruiterId { get; set; }
    public string? RecruiterName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateJobRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int? CompanyId { get; set; }
}

public class UpdateJobRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
