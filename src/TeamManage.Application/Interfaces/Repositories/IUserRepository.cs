using TeamManage.Domain.Entities;

namespace TeamManage.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);

    Task<User?> GetByInviteTokenHashAsync(string tokenHash, CancellationToken ct = default);

    Task AddAsync(User user, CancellationToken ct = default);

    Task<List<User>> GetTeamManagersCreatedByAsync(Guid creatorId, CancellationToken ct = default);

    Task<User?> GetTeamManagerCreatedByAsync(Guid id, Guid creatorId, CancellationToken ct = default);

    void Remove(User user);
}
