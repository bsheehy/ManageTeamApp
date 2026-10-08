using TeamManage.Domain.Entities;

namespace TeamManage.Application.Interfaces.Repositories;

public interface ITeamRepository
{
    /// <summary>
    /// Gets a team including its players (and their user records) and positions.
    /// </summary>
    Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<List<Team>> GetByManagerIdAsync(Guid managerId, CancellationToken ct = default);

    Task<List<Team>> GetByPlayerIdAsync(Guid playerId, CancellationToken ct = default);

    Task AddAsync(Team team, CancellationToken ct = default);

    Task<TeamPlayer?> GetTeamPlayerAsync(Guid teamId, Guid playerId, CancellationToken ct = default);

    Task<TeamPlayer?> GetTeamPlayerByIdAsync(Guid teamPlayerId, CancellationToken ct = default);

    Task AddPlayerAsync(TeamPlayer teamPlayer, CancellationToken ct = default);

    void RemovePlayer(TeamPlayer teamPlayer);
}
