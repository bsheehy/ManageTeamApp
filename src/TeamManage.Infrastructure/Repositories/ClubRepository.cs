using Microsoft.EntityFrameworkCore;
using TeamManage.Application.Interfaces.Repositories;
using TeamManage.Domain.Entities;
using TeamManage.Infrastructure.Persistence;

namespace TeamManage.Infrastructure.Repositories;

public class ClubRepository : IClubRepository
{
    private readonly TeamManageDbContext _context;

    public ClubRepository(TeamManageDbContext context)
    {
        _context = context;
    }

    public Task<Club?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Clubs
            .Include(c => c.Teams)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<Club>> GetByClubManagerIdAsync(Guid clubManagerId, CancellationToken ct = default) =>
        _context.Clubs
            .Include(c => c.Teams)
            .Where(c => c.ClubManagerId == clubManagerId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task AddAsync(Club club, CancellationToken ct = default) =>
        await _context.Clubs.AddAsync(club, ct);
}
