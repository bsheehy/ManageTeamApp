using Microsoft.EntityFrameworkCore;
using TeamManage.Application.Interfaces.Repositories;
using TeamManage.Domain.Entities;
using TeamManage.Infrastructure.Persistence;

namespace TeamManage.Infrastructure.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly TeamManageDbContext _context;

    public TeamRepository(TeamManageDbContext context)
    {
        _context = context;
    }

    public Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Teams
            .Include(t => t.Club)
            .Include(t => t.Manager)
            .Include(t => t.Players).ThenInclude(p => p.Player)
            .Include(t => t.Positions)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<List<Team>> GetForUserAsync(Guid userId, CancellationToken ct = default) =>
        _context.Teams
            .Include(t => t.Club)
            .Include(t => t.Players)
            .Where(t => t.ManagerId == userId
                || t.Club!.ClubManagerId == userId
                || t.Players.Any(p => p.PlayerId == userId))
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

    public Task<List<Team>> GetByClubIdAsync(Guid clubId, CancellationToken ct = default) =>
        _context.Teams
            .Include(t => t.Club)
            .Include(t => t.Players)
            .Where(t => t.ClubId == clubId)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

    public async Task AddAsync(Team team, CancellationToken ct = default) =>
        await _context.Teams.AddAsync(team, ct);

    public Task<TeamPlayer?> GetTeamPlayerAsync(Guid teamId, Guid playerId, CancellationToken ct = default) =>
        _context.TeamPlayers.FirstOrDefaultAsync(tp => tp.TeamId == teamId && tp.PlayerId == playerId, ct);

    public Task<TeamPlayer?> GetTeamPlayerByIdAsync(Guid teamPlayerId, CancellationToken ct = default) =>
        _context.TeamPlayers.FirstOrDefaultAsync(tp => tp.Id == teamPlayerId, ct);

    public async Task AddPlayerAsync(TeamPlayer teamPlayer, CancellationToken ct = default) =>
        await _context.TeamPlayers.AddAsync(teamPlayer, ct);

    public void RemovePlayer(TeamPlayer teamPlayer) =>
        _context.TeamPlayers.Remove(teamPlayer);
}
