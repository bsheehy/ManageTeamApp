using System.Net.Http.Json;
using TeamManage.Application.DTOs.Teams;

namespace TeamManage.App.Services;

public interface ITeamApiService
{
    Task<List<TeamSummaryDto>> GetMyTeamsAsync();

    Task<TeamDto> GetTeamAsync(Guid teamId);

    Task<TeamDto> CreateTeamAsync(CreateTeamRequest request);

    Task<TeamPlayerDto> AddPlayerAsync(Guid teamId, AddPlayerRequest request);

    Task RemovePlayerAsync(Guid teamId, Guid teamPlayerId);
}

public class TeamApiService : ApiClientBase, ITeamApiService
{
    public TeamApiService(HttpClient http) : base(http)
    {
    }

    public async Task<List<TeamSummaryDto>> GetMyTeamsAsync()
    {
        var response = await Http.GetAsync("api/teams");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<List<TeamSummaryDto>>()) ?? [];
    }

    public async Task<TeamDto> GetTeamAsync(Guid teamId)
    {
        var response = await Http.GetAsync($"api/teams/{teamId}");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<TeamDto>())!;
    }

    public async Task<TeamDto> CreateTeamAsync(CreateTeamRequest request)
    {
        var response = await Http.PostAsJsonAsync("api/teams", request);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<TeamDto>())!;
    }

    public async Task<TeamPlayerDto> AddPlayerAsync(Guid teamId, AddPlayerRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/teams/{teamId}/players", request);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<TeamPlayerDto>())!;
    }

    public async Task RemovePlayerAsync(Guid teamId, Guid teamPlayerId)
    {
        var response = await Http.DeleteAsync($"api/teams/{teamId}/players/{teamPlayerId}");
        await EnsureSuccessAsync(response);
    }
}
