namespace JobApplication.Application.DTOs;

public class CandidateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CvUrl { get; set; } = string.Empty;
}

public class UpdateCandidateRequest
{
    public string Name { get; set; } = string.Empty;
    public string CvUrl { get; set; } = string.Empty;
}
