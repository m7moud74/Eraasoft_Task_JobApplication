using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace JobApplication.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ICandidateRepository _candidateRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ICandidateRepository candidateRepository,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _candidateRepository = candidateRepository;
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

        var emailNormalized = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _userManager.FindByEmailAsync(emailNormalized);
        if (existingUser is not null)
        {
            throw new BadRequestException("Email is already registered.");
        }

        var role = string.IsNullOrWhiteSpace(request.Role) ? "Candidate" : request.Role.Trim();
        if (!string.Equals(role, "Candidate", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(role, "Recruiter", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Role must be either 'Candidate' or 'Recruiter'.");
        }

        role = string.Equals(role, "Recruiter", StringComparison.OrdinalIgnoreCase) ? "Recruiter" : "Candidate";

        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole(role));
        }

        int? candidateId = null;
        Candidate? candidate = null;

        if (role == "Candidate")
        {
            candidate = new Candidate
            {
                Name = request.Name.Trim(),
                Email = emailNormalized,
                CvUrl = request.CvUrl?.Trim() ?? string.Empty
            };

            await _candidateRepository.InsertAsync(candidate, cancellationToken);
            await _candidateRepository.SaveChangesAsync(cancellationToken);
            candidateId = candidate.Id;
        }

        var user = new ApplicationUser
        {
            UserName = emailNormalized,
            Email = emailNormalized,
            CandidateId = candidateId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            if (candidate is not null)
            {
                _candidateRepository.Remove(candidate);
                await _candidateRepository.SaveChangesAsync(cancellationToken);
            }

            var errorMsg = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Registration failed: {errorMsg}");
        }

        await _userManager.AddToRoleAsync(user, role);

        var token = _jwtTokenService.GenerateToken(user.Id, user.Email!, role, candidateId);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            Name = candidate?.Name ?? request.Name.Trim(),
            Role = role,
            CandidateId = candidateId
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
        if (user.CandidateId.HasValue)
        {
            var candidate = await _candidateRepository.GetByIdAsync(user.CandidateId.Value, cancellationToken);
            if (candidate is not null)
            {
                name = candidate.Name;
            }
        }

        var token = _jwtTokenService.GenerateToken(user.Id, user.Email!, role, user.CandidateId);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            Name = name,
            Role = role,
            CandidateId = user.CandidateId
        };
    }
}
