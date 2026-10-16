using TeamManage.Application.Common;
using TeamManage.Application.DTOs.Clubs;

namespace TeamManage.Application.Interfaces.Services;

public interface IClubService
{
    Task<Result<ClubDto>> CreateClubAsync(Guid clubManagerId, CreateClubRequest request, CancellationToken ct = default);

    Task<Result<List<ClubDto>>> GetClubsForManagerAsync(Guid clubManagerId, CancellationToken ct = default);
}
