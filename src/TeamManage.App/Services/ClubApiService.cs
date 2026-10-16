using System.Net.Http.Json;
using TeamManage.Application.DTOs.Clubs;

namespace TeamManage.App.Services;

public interface IClubApiService
{
    Task<List<ClubDto>> GetMyClubsAsync();

    Task<ClubDto> CreateClubAsync(CreateClubRequest request);
}

public class ClubApiService : ApiClientBase, IClubApiService
{
    public ClubApiService(HttpClient http) : base(http)
    {
    }

    public async Task<List<ClubDto>> GetMyClubsAsync()
    {
        var response = await Http.GetAsync("api/clubs");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<List<ClubDto>>()) ?? [];
    }

    public async Task<ClubDto> CreateClubAsync(CreateClubRequest request)
    {
        var response = await Http.PostAsJsonAsync("api/clubs", request);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<ClubDto>())!;
    }
}
