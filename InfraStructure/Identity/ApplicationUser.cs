using JobApplication.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace JobApplication.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public int? CandidateId { get; set; }
    public Candidate? Candidate { get; set; }
}
