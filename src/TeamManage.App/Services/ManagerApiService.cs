using System.Net.Http.Json;
using TeamManage.Application.DTOs.Managers;

namespace TeamManage.App.Services;

public interface IManagerApiService
{
    Task<List<TeamManagerSummaryDto>> GetManagersAsync();

    Task CreateAsync(SaveTeamManagerRequest request);

    Task UpdateAsync(Guid id, SaveTeamManagerRequest request);

    Task DeleteAsync(Guid id);

    Task ResendInviteAsync(Guid id);
}

public class ManagerApiService : ApiClientBase, IManagerApiService
{
    public ManagerApiService(HttpClient http) : base(http)
    {
    }

    public async Task<List<TeamManagerSummaryDto>> GetManagersAsync()
    {
        var response = await Http.GetAsync("api/managers");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<List<TeamManagerSummaryDto>>()) ?? [];
    }

    public async Task CreateAsync(SaveTeamManagerRequest request)
    {
        var response = await Http.PostAsJsonAsync("api/managers", request);
        await EnsureSuccessAsync(response);
    }

    public async Task UpdateAsync(Guid id, SaveTeamManagerRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/managers/{id}", request);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await Http.DeleteAsync($"api/managers/{id}");
        await EnsureSuccessAsync(response);
    }

    public async Task ResendInviteAsync(Guid id)
    {
        var response = await Http.PostAsync($"api/managers/{id}/resend-invite", null);
        await EnsureSuccessAsync(response);
    }
}
