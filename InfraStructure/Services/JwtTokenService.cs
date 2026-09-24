using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JobApplication.Infrastructure.Services;

public interface IJwtTokenService
{
    string GenerateToken(string userId, string email, string role, int? candidateId, int? companyId = null, int? recruiterId = null, bool isCompanyOwner = false);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string userId, string email, string role, int? candidateId, int? companyId = null, int? recruiterId = null, bool isCompanyOwner = false)
    {
        var secretKey = _configuration["Jwt:Key"] ?? "TrackApplicationSecureJwtSigningKeyForDevelopment2026!";
        var issuer = _configuration["Jwt:Issuer"] ?? "JobApplicationApi";
        var audience = _configuration["Jwt:Audience"] ?? "JobApplicationUsers";
        var expiryMinutes = int.TryParse(_configuration["Jwt:DurationInMinutes"], out var minutes) ? minutes : 120;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        if (candidateId.HasValue)
        {
            claims.Add(new Claim("candidate_id", candidateId.Value.ToString()));
            claims.Add(new Claim("CandidateId", candidateId.Value.ToString()));
        }

        if (companyId.HasValue)
        {
            claims.Add(new Claim("company_id", companyId.Value.ToString()));
            claims.Add(new Claim("CompanyId", companyId.Value.ToString()));
        }

        if (recruiterId.HasValue)
        {
            claims.Add(new Claim("recruiter_id", recruiterId.Value.ToString()));
            claims.Add(new Claim("RecruiterId", recruiterId.Value.ToString()));
        }

        if (isCompanyOwner)
        {
            claims.Add(new Claim("is_company_owner", "true"));
            claims.Add(new Claim("IsCompanyOwner", "true"));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
