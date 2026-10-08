using TeamManage.Application.Common;
using TeamManage.Application.DTOs.Matches;
using TeamManage.Application.DTOs.Teams;
using TeamManage.Application.Interfaces.Repositories;
using TeamManage.Application.Interfaces.Services;
using TeamManage.Domain.Entities;
using TeamManage.Domain.Enums;

namespace TeamManage.Application.Services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository _matches;
    private readonly ITeamRepository _teams;
    private readonly IUnitOfWork _unitOfWork;

    public MatchService(IMatchRepository matches, ITeamRepository teams, IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _teams = teams;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MatchDto>> CreateMatchAsync(Guid managerId, CreateMatchRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Opponent) || string.IsNullOrWhiteSpace(request.Location))
        {
            return Result<MatchDto>.Failure("Opponent and location are required.");
        }

        var team = await _teams.GetByIdAsync(request.TeamId, ct);
        if (team is null)
        {
            return Result<MatchDto>.Failure("Team not found.");
        }

        if (team.ManagerId != managerId)
        {
            return Result<MatchDto>.Failure("Only the team's manager can schedule matches.");
        }

        var match = new Match
        {
            TeamId = team.Id,
            Opponent = request.Opponent.Trim(),
            Location = request.Location.Trim(),
            KickOff = request.KickOff,
            Notes = request.Notes
        };

        // Invite every current panel member; they can confirm/decline attendance,
        // and the manager later allocates starters/substitutes from those confirmed.
        foreach (var teamPlayer in team.Players)
        {
            match.Selections.Add(new MatchPlayer
            {
                MatchId = match.Id,
                PlayerId = teamPlayer.PlayerId,
                Player = teamPlayer.Player
            });
        }

        await _matches.AddAsync(match, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<MatchDto>.Success(MapToDto(match, team.Name));
    }

    public async Task<Result<List<MatchSummaryDto>>> GetMatchesForTeamAsync(Guid teamId, CancellationToken ct = default)
    {
        var matches = await _matches.GetByTeamIdAsync(teamId, ct);
        return Result<List<MatchSummaryDto>>.Success(matches.Select(m => MapToSummaryDto(m, playerId: null)).ToList());
    }

    public async Task<Result<List<MatchSummaryDto>>> GetMatchesForPlayerAsync(Guid playerId, CancellationToken ct = default)
    {
        var matches = await _matches.GetByPlayerIdAsync(playerId, ct);
        return Result<List<MatchSummaryDto>>.Success(matches.Select(m => MapToSummaryDto(m, playerId)).ToList());
    }

    public async Task<Result<MatchDto>> GetMatchDetailAsync(Guid matchId, CancellationToken ct = default)
    {
        var match = await _matches.GetByIdAsync(matchId, ct);
        if (match is null)
        {
            return Result<MatchDto>.Failure("Match not found.");
        }

        return Result<MatchDto>.Success(MapToDto(match, match.Team?.Name ?? string.Empty));
    }

    public async Task<Result> SetAttendanceAsync(Guid matchId, Guid playerId, AttendanceStatus status, CancellationToken ct = default)
    {
        var matchPlayer = await _matches.GetMatchPlayerAsync(matchId, playerId, ct);
        if (matchPlayer is null)
        {
            return Result.Failure("You have not been invited to this match.");
        }

        matchPlayer.AttendanceStatus = status;
        matchPlayer.RespondedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<MatchDto>> AllocateSelectionsAsync(Guid matchId, Guid managerId, List<PlayerAllocationRequest> allocations, CancellationToken ct = default)
    {
        var match = await _matches.GetByIdAsync(matchId, ct);
        if (match is null)
        {
            return Result<MatchDto>.Failure("Match not found.");
        }

        if (match.Team is null || match.Team.ManagerId != managerId)
        {
            return Result<MatchDto>.Failure("Only the team's manager can set the match-day squad.");
        }

        var starterCount = allocations.Count(a => a.IsStarter);
        if (starterCount > match.Team.SquadSize)
        {
            return Result<MatchDto>.Failure($"The starting lineup cannot exceed {match.Team.SquadSize} players.");
        }

        var starterPositionIds = allocations
            .Where(a => a.IsStarter && a.PositionId.HasValue)
            .Select(a => a.PositionId!.Value)
            .ToList();
        if (starterPositionIds.Count != starterPositionIds.Distinct().Count())
        {
            return Result<MatchDto>.Failure("Each position can only be assigned to one starting player.");
        }

        foreach (var allocation in allocations)
        {
            var matchPlayer = match.Selections.FirstOrDefault(s => s.Id == allocation.MatchPlayerId);
            if (matchPlayer is null)
            {
                continue;
            }

            matchPlayer.PositionId = allocation.PositionId;
            matchPlayer.IsStarter = allocation.IsStarter;
            matchPlayer.IsSelected = allocation.IsSelected || allocation.IsStarter;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<MatchDto>.Success(MapToDto(match, match.Team.Name));
    }

    private static MatchSummaryDto MapToSummaryDto(Match match, Guid? playerId) => new()
    {
        Id = match.Id,
        TeamId = match.TeamId,
        TeamName = match.Team?.Name ?? string.Empty,
        Opponent = match.Opponent,
        Location = match.Location,
        KickOff = match.KickOff,
        Status = match.Status,
        MyAttendanceStatus = playerId.HasValue
            ? match.Selections.FirstOrDefault(s => s.PlayerId == playerId.Value)?.AttendanceStatus
            : null
    };

    private static MatchDto MapToDto(Match match, string teamName) => new()
    {
        Id = match.Id,
        TeamId = match.TeamId,
        TeamName = teamName,
        Opponent = match.Opponent,
        Location = match.Location,
        KickOff = match.KickOff,
        Status = match.Status,
        Notes = match.Notes,
        Selections = match.Selections
            .OrderBy(s => s.Position != null ? s.Position.SortOrder : int.MaxValue)
            .Select(s => new MatchPlayerDto
            {
                MatchPlayerId = s.Id,
                PlayerId = s.PlayerId,
                PlayerName = s.Player?.FullName ?? string.Empty,
                PositionId = s.PositionId,
                PositionName = s.Position?.Name,
                IsStarter = s.IsStarter,
                IsSelected = s.IsSelected,
                AttendanceStatus = s.AttendanceStatus,
                RespondedAt = s.RespondedAt
            }).ToList(),
        TeamPositions = (match.Team?.Positions ?? [])
            .OrderBy(p => p.SortOrder)
            .Select(p => new PositionDto { Id = p.Id, Name = p.Name, SortOrder = p.SortOrder })
            .ToList()
    };
}
