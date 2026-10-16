using TeamManage.Domain.Entities;

namespace TeamManage.Application.Interfaces.Repositories;

public interface ITeamRepository
{
    /// <summary>
    /// Gets a team including its players (and their user records) and positions.
    /// </summary>
    Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets every team the user is involved in: teams in clubs they manage, teams they
    /// manage, and teams they play on.
    /// </summary>
    Task<List<Team>> GetForUserAsync(Guid userId, CancellationToken ct = default);

    Task<List<Team>> GetByClubIdAsync(Guid clubId, CancellationToken ct = default);

    Task AddAsync(Team team, CancellationToken ct = default);

    Task<TeamPlayer?> GetTeamPlayerAsync(Guid teamId, Guid playerId, CancellationToken ct = default);

    Task<TeamPlayer?> GetTeamPlayerByIdAsync(Guid teamPlayerId, CancellationToken ct = default);

    Task AddPlayerAsync(TeamPlayer teamPlayer, CancellationToken ct = default);

    void RemovePlayer(TeamPlayer teamPlayer);
}
