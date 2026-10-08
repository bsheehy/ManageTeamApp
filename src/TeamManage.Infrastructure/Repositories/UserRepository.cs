using Microsoft.EntityFrameworkCore;
using TeamManage.Application.Interfaces.Repositories;
using TeamManage.Domain.Entities;
using TeamManage.Infrastructure.Persistence;

namespace TeamManage.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TeamManageDbContext _context;

    public UserRepository(TeamManageDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
        _context.Users.AnyAsync(u => u.Email == email, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await _context.Users.AddAsync(user, ct);
}
