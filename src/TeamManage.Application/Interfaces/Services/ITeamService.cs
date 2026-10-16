using TeamManage.Application.Common;
using TeamManage.Application.DTOs.Teams;

namespace TeamManage.Application.Interfaces.Services;

public interface ITeamService
{
    Task<Result<TeamDto>> CreateTeamAsync(Guid userId, CreateTeamRequest request, CancellationToken ct = default);

    Task<Result<List<TeamSummaryDto>>> GetTeamsForUserAsync(Guid userId, CancellationToken ct = default);

    Task<Result<TeamDto>> GetTeamDetailAsync(Guid teamId, Guid userId, CancellationToken ct = default);

    Task<Result<TeamManagerDto>> AssignTeamManagerAsync(Guid teamId, Guid clubManagerId, AssignManagerRequest request, CancellationToken ct = default);

    Task<Result<TeamPlayerDto>> AddPlayerToTeamAsync(Guid teamId, Guid managerId, AddPlayerRequest request, CancellationToken ct = default);

    Task<Result> RemovePlayerFromTeamAsync(Guid teamId, Guid managerId, Guid teamPlayerId, CancellationToken ct = default);
}
