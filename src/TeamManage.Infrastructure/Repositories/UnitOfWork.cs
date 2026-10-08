using TeamManage.Application.Interfaces.Repositories;
using TeamManage.Infrastructure.Persistence;

namespace TeamManage.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TeamManageDbContext _context;

    public UnitOfWork(TeamManageDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);
}
