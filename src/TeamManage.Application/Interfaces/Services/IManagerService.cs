using TeamManage.Application.Common;
using TeamManage.Application.DTOs.Managers;

namespace TeamManage.Application.Interfaces.Services;

public interface IManagerService
{
    Task<Result<List<TeamManagerSummaryDto>>> GetManagersAsync(Guid clubManagerId, CancellationToken ct = default);

    Task<Result<TeamManagerSummaryDto>> CreateManagerAsync(Guid clubManagerId, SaveTeamManagerRequest request, CancellationToken ct = default);

    Task<Result<TeamManagerSummaryDto>> UpdateManagerAsync(Guid clubManagerId, Guid managerId, SaveTeamManagerRequest request, CancellationToken ct = default);

    Task<Result> DeleteManagerAsync(Guid clubManagerId, Guid managerId, CancellationToken ct = default);

    Task<Result> ResendInviteAsync(Guid clubManagerId, Guid managerId, CancellationToken ct = default);
}
