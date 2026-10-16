using System.Security.Cryptography;
using TeamManage.Application.Common;
using TeamManage.Application.DTOs.Managers;
using TeamManage.Application.Interfaces.Repositories;
using TeamManage.Application.Interfaces.Security;
using TeamManage.Application.Interfaces.Services;
using TeamManage.Domain.Entities;
using TeamManage.Domain.Enums;

namespace TeamManage.Application.Services;

public class ManagerService : IManagerService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IEmailSender _email;

    public ManagerService(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IEmailSender email)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _email = email;
    }

    public async Task<Result<List<TeamManagerSummaryDto>>> GetManagersAsync(Guid clubManagerId, CancellationToken ct = default)
    {
        var managers = await _users.GetTeamManagersCreatedByAsync(clubManagerId, ct);
        return Result<List<TeamManagerSummaryDto>>.Success(managers.Select(MapToDto).ToList());
    }

    public async Task<Result<TeamManagerSummaryDto>> CreateManagerAsync(Guid clubManagerId, SaveTeamManagerRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<TeamManagerSummaryDto>.Failure("Name and email are required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _users.ExistsByEmailAsync(email, ct))
        {
            return Result<TeamManagerSummaryDto>.Failure("An account with this email already exists.");
        }

        var clubManager = await _users.GetByIdAsync(clubManagerId, ct);
        if (clubManager is null || clubManager.Role != UserRole.ClubManager)
        {
            return Result<TeamManagerSummaryDto>.Failure("Only club managers can create team managers.");
        }

        var manager = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(GenerateRandomPassword()),
            Role = UserRole.TeamManager,
            MustChangePassword = true,
            CreatedByUserId = clubManagerId
        };
        var token = ApplyNewInviteToken(manager);

        await _users.AddAsync(manager, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await SendInviteAsync(manager, clubManager.FullName, token, ct);

        return Result<TeamManagerSummaryDto>.Success(MapToDto(manager));
    }

    public async Task<Result<TeamManagerSummaryDto>> UpdateManagerAsync(Guid clubManagerId, Guid managerId, SaveTeamManagerRequest request, CancellationToken ct = default)
    {
        var manager = await _users.GetTeamManagerCreatedByAsync(managerId, clubManagerId, ct);
        if (manager is null)
        {
            return Result<TeamManagerSummaryDto>.Failure("Team manager not found.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<TeamManagerSummaryDto>.Failure("Name and email are required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (email != manager.Email && await _users.ExistsByEmailAsync(email, ct))
        {
            return Result<TeamManagerSummaryDto>.Failure("An account with this email already exists.");
        }

        manager.FullName = request.FullName.Trim();
        manager.Email = email;
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<TeamManagerSummaryDto>.Success(MapToDto(manager));
    }

    public async Task<Result> DeleteManagerAsync(Guid clubManagerId, Guid managerId, CancellationToken ct = default)
    {
        var manager = await _users.GetTeamManagerCreatedByAsync(managerId, clubManagerId, ct);
        if (manager is null)
        {
            return Result.Failure("Team manager not found.");
        }

        if (manager.ManagedTeams.Count > 0)
        {
            return Result.Failure("This manager is assigned to a team. Assign another manager to those teams first.");
        }

        _users.Remove(manager);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> ResendInviteAsync(Guid clubManagerId, Guid managerId, CancellationToken ct = default)
    {
        var manager = await _users.GetTeamManagerCreatedByAsync(managerId, clubManagerId, ct);
        if (manager is null)
        {
            return Result.Failure("Team manager not found.");
        }

        if (!manager.MustChangePassword)
        {
            return Result.Failure("This manager has already set up their account.");
        }

        var clubManager = await _users.GetByIdAsync(clubManagerId, ct);
        var token = ApplyNewInviteToken(manager);
        await _unitOfWork.SaveChangesAsync(ct);
        await SendInviteAsync(manager, clubManager?.FullName ?? "your club manager", token, ct);

        return Result.Success();
    }

    private string ApplyNewInviteToken(User manager)
    {
        var (token, _) = _tokenGenerator.GenerateRefreshToken();
        manager.InviteTokenHash = _tokenGenerator.HashRefreshToken(token);
        manager.InviteTokenExpiresAtUtc = DateTime.UtcNow.AddDays(7);
        return token;
    }

    private Task SendInviteAsync(User manager, string clubManagerName, string token, CancellationToken ct)
    {
        var link = $"{InviteLink.Scheme}://login?token={Uri.EscapeDataString(token)}";
        return _email.SendAsync(
            manager.Email,
            "You've been added as a team manager",
            $"Hi {manager.FullName},\n\n{clubManagerName} has added you as a team manager.\n\n" +
            $"Your login email: {manager.Email}\n\n" +
            $"Open this link on your phone to sign in and set your password (expires in 7 days):\n{link}\n",
            ct);
    }

    private static string GenerateRandomPassword() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));

    private static TeamManagerSummaryDto MapToDto(User manager) => new()
    {
        Id = manager.Id,
        FullName = manager.FullName,
        Email = manager.Email,
        TeamCount = manager.ManagedTeams.Count,
        InvitePending = manager.MustChangePassword
    };
}
