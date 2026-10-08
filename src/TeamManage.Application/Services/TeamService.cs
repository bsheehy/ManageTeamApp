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
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public TeamService(
        ITeamRepository teams,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _teams = teams;
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<TeamDto>> CreateTeamAsync(Guid managerId, CreateTeamRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<TeamDto>.Failure("Team name is required.");
        }

        if (request.SquadSize <= 0)
        {
            return Result<TeamDto>.Failure("Squad size must be greater than zero.");
        }

        var manager = await _users.GetByIdAsync(managerId, ct);
        if (manager is null || manager.Role != UserRole.Manager)
        {
            return Result<TeamDto>.Failure("Only managers can create teams.");
        }

        var team = new Team
        {
            Name = request.Name.Trim(),
            Sport = request.Sport,
            SquadSize = request.SquadSize,
            ManagerId = managerId
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

        return Result<TeamDto>.Success(MapToDto(team, manager.FullName));
    }

    public async Task<Result<List<TeamSummaryDto>>> GetTeamsForManagerAsync(Guid managerId, CancellationToken ct = default)
    {
        var teams = await _teams.GetByManagerIdAsync(managerId, ct);
        return Result<List<TeamSummaryDto>>.Success(teams.Select(MapToSummaryDto).ToList());
    }

    public async Task<Result<List<TeamSummaryDto>>> GetTeamsForPlayerAsync(Guid playerId, CancellationToken ct = default)
    {
        var teams = await _teams.GetByPlayerIdAsync(playerId, ct);
        return Result<List<TeamSummaryDto>>.Success(teams.Select(MapToSummaryDto).ToList());
    }

    public async Task<Result<TeamDto>> GetTeamDetailAsync(Guid teamId, CancellationToken ct = default)
    {
        var team = await _teams.GetByIdAsync(teamId, ct);
        if (team is null)
        {
            return Result<TeamDto>.Failure("Team not found.");
        }

        return Result<TeamDto>.Success(MapToDto(team, team.Manager?.FullName ?? string.Empty));
    }

    public async Task<Result<TeamPlayerDto>> AddPlayerToTeamAsync(Guid teamId, Guid managerId, AddPlayerRequest request, CancellationToken ct = default)
    {
        var team = await _teams.GetByIdAsync(teamId, ct);
        if (team is null)
        {
            return Result<TeamPlayerDto>.Failure("Team not found.");
        }

        if (team.ManagerId != managerId)
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

        if (team.ManagerId != managerId)
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

    private static TeamSummaryDto MapToSummaryDto(Team team) => new()
    {
        Id = team.Id,
        Name = team.Name,
        Sport = team.Sport,
        SquadSize = team.SquadSize,
        PlayerCount = team.Players.Count
    };

    private static TeamDto MapToDto(Team team, string managerName) => new()
    {
        Id = team.Id,
        Name = team.Name,
        Sport = team.Sport,
        SquadSize = team.SquadSize,
        ManagerId = team.ManagerId,
        ManagerName = managerName,
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
