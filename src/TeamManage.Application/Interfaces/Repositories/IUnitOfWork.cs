namespace TeamManage.Application.Interfaces.Repositories;

/// <summary>
/// Commits changes made across one or more repositories within a single transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
