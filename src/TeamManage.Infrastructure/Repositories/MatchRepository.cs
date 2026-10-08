using Microsoft.EntityFrameworkCore;
using TeamManage.Application.Interfaces.Repositories;
using TeamManage.Domain.Entities;
using TeamManage.Infrastructure.Persistence;

namespace TeamManage.Infrastructure.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly TeamManageDbContext _context;

    public MatchRepository(TeamManageDbContext context)
    {
        _context = context;
    }

    public Task<Match?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Matches
            .Include(m => m.Team).ThenInclude(t => t!.Positions)
            .Include(m => m.Selections).ThenInclude(s => s.Player)
            .Include(m => m.Selections).ThenInclude(s => s.Position)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<List<Match>> GetByTeamIdAsync(Guid teamId, CancellationToken ct = default) =>
        _context.Matches
            .Include(m => m.Team)
            .Where(m => m.TeamId == teamId)
            .OrderBy(m => m.KickOff)
            .ToListAsync(ct);

    public Task<List<Match>> GetByPlayerIdAsync(Guid playerId, CancellationToken ct = default) =>
        _context.Matches
            .Include(m => m.Team)
            .Include(m => m.Selections)
            .Where(m => m.Selections.Any(s => s.PlayerId == playerId))
            .OrderBy(m => m.KickOff)
            .ToListAsync(ct);

    public async Task AddAsync(Match match, CancellationToken ct = default) =>
        await _context.Matches.AddAsync(match, ct);

    public Task<MatchPlayer?> GetMatchPlayerAsync(Guid matchId, Guid playerId, CancellationToken ct = default) =>
        _context.MatchPlayers.FirstOrDefaultAsync(mp => mp.MatchId == matchId && mp.PlayerId == playerId, ct);
}
