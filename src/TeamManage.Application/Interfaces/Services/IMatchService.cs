using TeamManage.Application.Common;
using TeamManage.Application.DTOs.Matches;
using TeamManage.Domain.Enums;

namespace TeamManage.Application.Interfaces.Services;

public interface IMatchService
{
    Task<Result<MatchDto>> CreateMatchAsync(Guid managerId, CreateMatchRequest request, CancellationToken ct = default);

    Task<Result<List<MatchSummaryDto>>> GetMatchesForTeamAsync(Guid teamId, CancellationToken ct = default);

    Task<Result<List<MatchSummaryDto>>> GetMatchesForPlayerAsync(Guid playerId, CancellationToken ct = default);

    Task<Result<MatchDto>> GetMatchDetailAsync(Guid matchId, CancellationToken ct = default);

    Task<Result> SetAttendanceAsync(Guid matchId, Guid playerId, AttendanceStatus status, CancellationToken ct = default);

    Task<Result<MatchDto>> AllocateSelectionsAsync(Guid matchId, Guid managerId, List<PlayerAllocationRequest> allocations, CancellationToken ct = default);
}
