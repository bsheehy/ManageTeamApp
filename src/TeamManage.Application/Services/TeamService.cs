using System.Security.Cryptography;
using TeamManage.Application.Common;
using TeamManage.Application.DTOs.Teams;
using TeamManage.Application.Interfaces.Repositories;
using TeamManage.Application.Interfaces.Security;
using TeamManage.Application.Interfaces.Services;
using TeamManage.Domain.Entities;
using TeamManage.Domain.Enums;

namespace TeamManage.Application.Services;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _teams;
    private readonly IClubRepository _clubs;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IEmailSender _email;

    public TeamService(
        ITeamRepository teams,
        IClubRepository clubs,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IEmailSender email)
    {
        _tokenGenerator = tokenGenerator;
        _email = email;
        _teams = teams;
        _clubs = clubs;
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<TeamDto>> CreateTeamAsync(Guid userId, CreateTeamRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<TeamDto>.Failure("Team name is required.");
        }

        if (request.SquadSize <= 0)
        {
            return Result<TeamDto>.Failure("Squad size must be greater than zero.");
        }

        var club = await _clubs.GetByIdAsync(request.ClubId, ct);
        if (club is null)
        {
            return Result<TeamDto>.Failure("Club not found.");
        }

        if (club.ClubManagerId != userId)
        {
            return Result<TeamDto>.Failure("Only the club manager can create teams in this club.");
        }

        Guid? assignedManagerId = null;
        if (request.ManagerId.HasValue)
        {
            var teamManager = await _users.GetTeamManagerCreatedByAsync(request.ManagerId.Value, userId, ct);
            if (teamManager is null)
            {
                return Result<TeamDto>.Failure("Selected team manager was not found.");
            }

            assignedManagerId = teamManager.Id;
        }

        var team = new Team
        {
            Name = request.Name.Trim(),
            Sport = request.Sport,
            SquadSize = request.SquadSize,
            ClubId = club.Id,
            Club = club,
            ManagerId = assignedManagerId
        };

        var positionNames = request.PositionNames is { Count: > 0 }
            ? request.PositionNames
            : Enumerable.Range(1, request.SquadSize).Select(i => $"Position {i}").ToList();

        for (var i = 0; i < positionNames.Count; i++)
        {
            team.Positions.Add(new Position { Name = positionNames[i], SortOrder = i + 1, TeamId = team.Id });
        }

        await _teams.AddAsync(team, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<TeamDto>.Success(MapToDto(team, string.Empty, userId));
    }

    public async Task<Result<List<TeamSummaryDto>>> GetTeamsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var teams = await _teams.GetForUserAsync(userId, ct);
        return Result<List<TeamSummaryDto>>.Success(teams.Select(t => MapToSummaryDto(t, userId)).ToList());
    }

    public async Task<Result<TeamDto>> GetTeamDetailAsync(Guid teamId, Guid userId, CancellationToken ct = default)
    {
        var team = await _teams.GetByIdAsync(teamId, ct);
        if (team is null)
        {
            return Result<TeamDto>.Failure("Team not found.");
        }

        return Result<TeamDto>.Success(MapToDto(team, team.Manager?.FullName ?? string.Empty, userId));
    }

    public async Task<Result<TeamManagerDto>> AssignTeamManagerAsync(Guid teamId, Guid clubManagerId, AssignManagerRequest request, CancellationToken ct = default)
    {
        var team = await _teams.GetByIdAsync(teamId, ct);
        if (team is null)
        {
            return Result<TeamManagerDto>.Failure("Team not found.");
        }

        if (team.Club?.ClubManagerId != clubManagerId)
        {
            return Result<TeamManagerDto>.Failure("Only the club manager can assign a team manager.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<TeamManagerDto>.Failure("Manager email is required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var manager = await _users.GetByEmailAsync(email, ct);
        string? inviteToken = null;

        if (manager is null)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return Result<TeamManagerDto>.Failure("Manager name is required for a new account.");
            }

            var (token, _) = _tokenGenerator.GenerateRefreshToken();
            inviteToken = token;
            manager = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = _passwordHasher.Hash(GenerateTemporaryPassword()),
                Role = UserRole.TeamManager,
                InviteTokenHash = _tokenGenerator.HashRefreshToken(token),
                InviteTokenExpiresAtUtc = DateTime.UtcNow.AddDays(7),
                MustChangePassword = true,
                CreatedByUserId = clubManagerId
            };
            await _users.AddAsync(manager, ct);
        }
        else if (manager.Role == UserRole.Player)
        {
            manager.Role = UserRole.TeamManager;
        }

        team.ManagerId = manager.Id;
        team.Manager = manager;
        await _unitOfWork.SaveChangesAsync(ct);

        if (inviteToken is not null)
        {
            var link = $"{InviteLink.Scheme}://login?token={Uri.EscapeDataString(inviteToken)}";
            await _email.SendAsync(
                manager.Email,
                $"You've been assigned to manage {team.Name}",
                $"Hi {manager.FullName},\n\nYou've been assigned as team manager of {team.Name} ({team.Club?.Name}).\n\n" +
                $"Your login email: {manager.Email}\n\n" +
                $"Open this link on your phone to sign in and set your password (expires in 7 days):\n{link}\n",
                ct);
        }

        return Result<TeamManagerDto>.Success(new TeamManagerDto
        {
            ManagerId = manager.Id,
            FullName = manager.FullName,
            Email = manager.Email,
            InviteSent = inviteToken is not null
        });
    }

    public async Task<Result<TeamPlayerDto>> AddPlayerToTeamAsync(Guid teamId, Guid managerId, AddPlayerRequest request, CancellationToken ct = default)
    {
        var team = await _teams.GetByIdAsync(teamId, ct);
        if (team is null)
        {
            return Result<TeamPlayerDto>.Failure("Team not found.");
        }

        if (!team.IsManagedBy(managerId))
        {
            return Result<TeamPlayerDto>.Failure("Only the team's manager can add players.");
        }

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.FullName))
        {
            return Result<TeamPlayerDto>.Failure("Player name and email are required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var player = await _users.GetByEmailAsync(email, ct);
        string? generatedPassword = null;

        if (player is null)
        {
            generatedPassword = GenerateTemporaryPassword();
            player = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = _passwordHasher.Hash(generatedPassword),
                Role = UserRole.Player
            };
            await _users.AddAsync(player, ct);
        }

        var existingMembership = await _teams.GetTeamPlayerAsync(teamId, player.Id, ct);
        if (existingMembership is not null)
        {
            return Result<TeamPlayerDto>.Failure("This player is already on the panel.");
        }

        var teamPlayer = new TeamPlayer
        {
            TeamId = teamId,
            PlayerId = player.Id,
            JerseyNumber = request.JerseyNumber
        };

        await _teams.AddPlayerAsync(teamPlayer, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<TeamPlayerDto>.Success(new TeamPlayerDto
        {
            TeamPlayerId = teamPlayer.Id,
            PlayerId = player.Id,
            FullName = player.FullName,
            Email = player.Email,
            JerseyNumber = teamPlayer.JerseyNumber,
            GeneratedTemporaryPassword = generatedPassword
        });
    }

    public async Task<Result> RemovePlayerFromTeamAsync(Guid teamId, Guid managerId, Guid teamPlayerId, CancellationToken ct = default)
    {
        var team = await _teams.GetByIdAsync(teamId, ct);
        if (team is null)
        {
            return Result.Failure("Team not found.");
        }

        if (!team.IsManagedBy(managerId))
        {
            return Result.Failure("Only the team's manager can remove players.");
        }

        var teamPlayer = team.Players.FirstOrDefault(p => p.Id == teamPlayerId);
        if (teamPlayer is null)
        {
            return Result.Failure("Player is not on this panel.");
        }

        _teams.RemovePlayer(teamPlayer);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static string GenerateTemporaryPassword()
    {
        // 12 cryptographically random bytes, base64-url-encoded -> readable, unguessable temp password.
        var bytes = RandomNumberGenerator.GetBytes(12);
        return Convert.ToBase64String(bytes).Replace("+", "P").Replace("/", "s").Replace("=", string.Empty);
    }

    private static TeamSummaryDto MapToSummaryDto(Team team, Guid userId) => new()
    {
        Id = team.Id,
        Name = team.Name,
        Sport = team.Sport,
        SquadSize = team.SquadSize,
        PlayerCount = team.Players.Count,
        ClubId = team.ClubId,
        ClubName = team.Club?.Name ?? string.Empty,
        CanManage = team.IsManagedBy(userId)
    };

    private static TeamDto MapToDto(Team team, string managerName, Guid userId) => new()
    {
        Id = team.Id,
        Name = team.Name,
        Sport = team.Sport,
        SquadSize = team.SquadSize,
        ManagerId = team.ManagerId,
        ManagerName = managerName,
        ClubId = team.ClubId,
        ClubName = team.Club?.Name ?? string.Empty,
        CanManage = team.IsManagedBy(userId),
        CanAssignManager = team.Club?.ClubManagerId == userId,
        Players = team.Players
            .OrderBy(p => p.JerseyNumber ?? int.MaxValue)
            .Select(p => new TeamPlayerDto
            {
                TeamPlayerId = p.Id,
                PlayerId = p.PlayerId,
                FullName = p.Player?.FullName ?? string.Empty,
                Email = p.Player?.Email ?? string.Empty,
                JerseyNumber = p.JerseyNumber
            }).ToList(),
        Positions = team.Positions
            .OrderBy(p => p.SortOrder)
            .Select(p => new PositionDto { Id = p.Id, Name = p.Name, SortOrder = p.SortOrder })
            .ToList()
    };
}
