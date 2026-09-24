namespace JobApplication.Domain.Entities;

public class Candidate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CvUrl { get; set; } = string.Empty;
    public string? CvPublicId { get; set; }
}

