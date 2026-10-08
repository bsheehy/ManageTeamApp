using Microsoft.EntityFrameworkCore;
using TeamManage.Application.Interfaces.Repositories;
using TeamManage.Domain.Entities;
using TeamManage.Infrastructure.Persistence;

namespace TeamManage.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly TeamManageDbContext _context;

    public RefreshTokenRepository(TeamManageDbContext context)
    {
        _context = context;
    }

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default) =>
        await _context.RefreshTokens.AddAsync(token, ct);
}
