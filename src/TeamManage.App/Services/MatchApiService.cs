using System.Net.Http.Json;
using TeamManage.Application.DTOs.Matches;
using TeamManage.Domain.Enums;

namespace TeamManage.App.Services;

public interface IMatchApiService
{
    Task<List<MatchSummaryDto>> GetMyMatchesAsync(Guid? teamId = null);

    Task<MatchDto> GetMatchAsync(Guid matchId);

    Task<MatchDto> CreateMatchAsync(CreateMatchRequest request);

    Task SetAttendanceAsync(Guid matchId, AttendanceStatus status);

    Task<MatchDto> AllocateSelectionsAsync(Guid matchId, List<PlayerAllocationRequest> allocations);
}

public class MatchApiService : ApiClientBase, IMatchApiService
{
    public MatchApiService(HttpClient http) : base(http)
    {
    }

    public async Task<List<MatchSummaryDto>> GetMyMatchesAsync(Guid? teamId = null)
    {
        var url = teamId.HasValue ? $"api/matches?teamId={teamId}" : "api/matches";
        var response = await Http.GetAsync(url);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<List<MatchSummaryDto>>()) ?? [];
    }

    public async Task<MatchDto> GetMatchAsync(Guid matchId)
    {
        var response = await Http.GetAsync($"api/matches/{matchId}");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }

    public async Task<MatchDto> CreateMatchAsync(CreateMatchRequest request)
    {
        var response = await Http.PostAsJsonAsync("api/matches", request);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }

    public async Task SetAttendanceAsync(Guid matchId, AttendanceStatus status)
    {
        var response = await Http.PutAsJsonAsync($"api/matches/{matchId}/attendance", new AttendanceResponseRequest { Status = status });
        await EnsureSuccessAsync(response);
    }

    public async Task<MatchDto> AllocateSelectionsAsync(Guid matchId, List<PlayerAllocationRequest> allocations)
    {
        var response = await Http.PutAsJsonAsync($"api/matches/{matchId}/selections", new AllocateSelectionsRequest { Allocations = allocations });
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<MatchDto>())!;
    }
}
