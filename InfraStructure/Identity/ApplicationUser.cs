using JobApplication.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace JobApplication.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public int? CandidateId { get; set; }
    public Candidate? Candidate { get; set; }

    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    public int? RecruiterId { get; set; }
    public Recruiter? Recruiter { get; set; }
}
