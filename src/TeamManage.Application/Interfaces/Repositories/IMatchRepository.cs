using TeamManage.Domain.Entities;

namespace TeamManage.Application.Interfaces.Repositories;

public interface IMatchRepository
{
    /// <summary>
    /// Gets a match including its team and full selection list (players + positions).
    /// </summary>
    Task<Match?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<List<Match>> GetByTeamIdAsync(Guid teamId, CancellationToken ct = default);

    /// <summary>
    /// Gets every match a player has been invited to (i.e. has a <see cref="MatchPlayer"/> row for).
    /// </summary>
    Task<List<Match>> GetByPlayerIdAsync(Guid playerId, CancellationToken ct = default);

    Task AddAsync(Match match, CancellationToken ct = default);

    Task<MatchPlayer?> GetMatchPlayerAsync(Guid matchId, Guid playerId, CancellationToken ct = default);
}
