using TeamManage.Application.Common;
using TeamManage.Application.DTOs.Teams;

namespace TeamManage.Application.Interfaces.Services;

public interface ITeamService
{
    Task<Result<TeamDto>> CreateTeamAsync(Guid managerId, CreateTeamRequest request, CancellationToken ct = default);

    Task<Result<List<TeamSummaryDto>>> GetTeamsForManagerAsync(Guid managerId, CancellationToken ct = default);

    Task<Result<List<TeamSummaryDto>>> GetTeamsForPlayerAsync(Guid playerId, CancellationToken ct = default);

    Task<Result<TeamDto>> GetTeamDetailAsync(Guid teamId, CancellationToken ct = default);

    Task<Result<TeamPlayerDto>> AddPlayerToTeamAsync(Guid teamId, Guid managerId, AddPlayerRequest request, CancellationToken ct = default);

    Task<Result> RemovePlayerFromTeamAsync(Guid teamId, Guid managerId, Guid teamPlayerId, CancellationToken ct = default);
}
