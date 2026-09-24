using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Command.Companies;

public record RegisterCompanyCommand(RegisterCompanyRequest Request) : IRequest<CompanyDto>;

public class RegisterCompanyCommandHandler : IRequestHandler<RegisterCompanyCommand, CompanyDto>
{
    private readonly IAuthService _authService;

    public RegisterCompanyCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<CompanyDto> Handle(RegisterCompanyCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RegisterCompanyAsync(request.Request, cancellationToken);
    }
}
