using TeamManage.Domain.Entities;

namespace TeamManage.Application.Interfaces.Repositories;

public interface IClubRepository
{
    Task<Club?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<List<Club>> GetByClubManagerIdAsync(Guid clubManagerId, CancellationToken ct = default);

    Task AddAsync(Club club, CancellationToken ct = default);
}
