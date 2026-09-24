using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using JobApplication.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace JobApplication.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ICandidateRepository _candidateRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IRecruiterRepository _recruiterRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ICandidateRepository candidateRepository,
        ICompanyRepository companyRepository,
        IRecruiterRepository recruiterRepository,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _candidateRepository = candidateRepository;
        _companyRepository = companyRepository;
        _recruiterRepository = recruiterRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BadRequestException("Email and password are required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Name is required.");
        }

        if (string.Equals(request.Role?.Trim(), "Recruiter", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Recruiters cannot register independently. A company must add its recruiters.");
        }

        var emailNormalized = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _userManager.FindByEmailAsync(emailNormalized);
        if (existingUser is not null)
        {
            throw new BadRequestException("Email is already registered.");
        }

        var role = "Candidate";

        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole(role));
        }

        var candidate = new Candidate
        {
            Name = request.Name.Trim(),
            Email = emailNormalized,
            CvUrl = request.CvUrl?.Trim() ?? string.Empty
        };

        await _candidateRepository.InsertAsync(candidate, cancellationToken);
        await _candidateRepository.SaveChangesAsync(cancellationToken);

        var user = new ApplicationUser
        {
            UserName = emailNormalized,
            Email = emailNormalized,
            CandidateId = candidate.Id
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            _candidateRepository.Remove(candidate);
            await _candidateRepository.SaveChangesAsync(cancellationToken);

            var errorMsg = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Registration failed: {errorMsg}");
        }

        await _userManager.AddToRoleAsync(user, role);

        var token = _jwtTokenService.GenerateToken(user.Id, user.Email!, role, candidate.Id);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            Name = candidate.Name,
            Role = role,
            CandidateId = candidate.Id
        };
    }

    public async Task<CompanyDto> RegisterCompanyAsync(RegisterCompanyRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Company name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.OwnerName))
        {
            throw new BadRequestException("Company owner/contact name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BadRequestException("Owner email and password are required.");
        }

        var emailNormalized = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _userManager.FindByEmailAsync(emailNormalized);
        if (existingUser is not null)
        {
            throw new BadRequestException("Email is already registered.");
        }

        if (!await _roleManager.RoleExistsAsync("Company"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Company"));
        }

        if (!await _roleManager.RoleExistsAsync("Recruiter"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Recruiter"));
        }

        var company = new Company
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            LogoUrl = request.LogoUrl?.Trim(),
            Status = CompanyStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _companyRepository.InsertAsync(company, cancellationToken);
        await _companyRepository.SaveChangesAsync(cancellationToken);

        var user = new ApplicationUser
        {
            UserName = emailNormalized,
            Email = emailNormalized,
            CompanyId = company.Id
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            _companyRepository.Remove(company);
            await _companyRepository.SaveChangesAsync(cancellationToken);

            var errorMsg = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Company registration failed: {errorMsg}");
        }

        await _userManager.AddToRoleAsync(user, "Company");
        await _userManager.AddToRoleAsync(user, "Recruiter");

        var recruiter = new Recruiter
        {
            Name = request.OwnerName.Trim(),
            Email = emailNormalized,
            CompanyId = company.Id,
            UserId = user.Id,
            IsCompanyOwner = true,
            CreatedAt = DateTime.UtcNow
        };

        await _recruiterRepository.InsertAsync(recruiter, cancellationToken);
        await _recruiterRepository.SaveChangesAsync(cancellationToken);

        user.RecruiterId = recruiter.Id;
        await _userManager.UpdateAsync(user);

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Description = company.Description,
            LogoUrl = company.LogoUrl,
            Status = company.Status,
            CreatedAt = company.CreatedAt,
            ApprovedAt = company.ApprovedAt,
            ApprovedByUserId = company.ApprovedByUserId,
            RecruitersCount = 1
        };
    }

    public async Task<RecruiterDto> AddRecruiterAsync(AddRecruiterRequest request, int companyId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Recruiter name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BadRequestException("Recruiter email and password are required.");
        }

        var emailNormalized = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _userManager.FindByEmailAsync(emailNormalized);
        if (existingUser is not null)
        {
            throw new BadRequestException("Email is already registered.");
        }

        var company = await _companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with ID {companyId} was not found.");
        }

        if (!await _roleManager.RoleExistsAsync("Recruiter"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Recruiter"));
        }

        var user = new ApplicationUser
        {
            UserName = emailNormalized,
            Email = emailNormalized,
            CompanyId = company.Id
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errorMsg = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Failed to create recruiter account: {errorMsg}");
        }

        await _userManager.AddToRoleAsync(user, "Recruiter");

        var recruiter = new Recruiter
        {
            Name = request.Name.Trim(),
            Email = emailNormalized,
            CompanyId = company.Id,
            UserId = user.Id,
            IsCompanyOwner = false,
            CreatedAt = DateTime.UtcNow
        };

        await _recruiterRepository.InsertAsync(recruiter, cancellationToken);
        await _recruiterRepository.SaveChangesAsync(cancellationToken);

        user.RecruiterId = recruiter.Id;
        await _userManager.UpdateAsync(user);

        return new RecruiterDto
        {
            Id = recruiter.Id,
            Name = recruiter.Name,
            Email = recruiter.Email,
            CompanyId = recruiter.CompanyId,
            CompanyName = company.Name,
            UserId = recruiter.UserId,
            IsCompanyOwner = recruiter.IsCompanyOwner,
            CreatedAt = recruiter.CreatedAt
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BadRequestException("Email and password are required.");
        }

        var emailNormalized = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(emailNormalized);
        if (user is null)
        {
            throw new BadRequestException("Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new BadRequestException("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Candidate";

        string name = user.UserName ?? user.Email ?? string.Empty;
        int? companyId = user.CompanyId;
        int? recruiterId = user.RecruiterId;
        bool isCompanyOwner = false;

        if (user.CandidateId.HasValue)
        {
            var candidate = await _candidateRepository.GetByIdAsync(user.CandidateId.Value, cancellationToken);
            if (candidate is not null)
            {
                name = candidate.Name;
            }
        }
        else
        {
            var recruiter = recruiterId.HasValue
                ? await _recruiterRepository.GetByIdAsync(recruiterId.Value, cancellationToken)
                : await _recruiterRepository.GetByUserIdAsync(user.Id, cancellationToken);

            if (recruiter is not null)
            {
                recruiterId = recruiter.Id;
                companyId = recruiter.CompanyId;
                isCompanyOwner = recruiter.IsCompanyOwner;
                name = recruiter.Name;
            }
        }

        if (companyId.HasValue)
        {
            var company = await _companyRepository.GetByIdAsync(companyId.Value, cancellationToken);
            if (company is not null)
            {
                if (company.Status == CompanyStatus.Pending)
                {
                    throw new BadRequestException("Your company registration is pending approval by an administrator.");
                }
                if (company.Status == CompanyStatus.Rejected)
                {
                    throw new BadRequestException("Your company registration has been rejected by an administrator.");
                }
            }
        }

        var token = _jwtTokenService.GenerateToken(
            user.Id,
            user.Email!,
            role,
            user.CandidateId,
            companyId,
            recruiterId,
            isCompanyOwner);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            Name = name,
            Role = role,
            CandidateId = user.CandidateId,
            CompanyId = companyId,
            RecruiterId = recruiterId
        };
    }
}
