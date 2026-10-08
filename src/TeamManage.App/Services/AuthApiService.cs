using System.Net.Http.Json;
using TeamManage.Application.DTOs.Auth;
using TeamManage.Domain.Enums;

namespace TeamManage.App.Services;

public interface IAuthApiService
{
    Task<AuthResponse> LoginAsync(string email, string password);

    Task<AuthResponse> RegisterAsync(string fullName, string email, string password, UserRole role);
}

public class AuthApiService : ApiClientBase, IAuthApiService
{
    public AuthApiService(HttpClient http) : base(http)
    {
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var response = await Http.PostAsJsonAsync("api/auth/login", new LoginRequest { Email = email, Password = password });
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    public async Task<AuthResponse> RegisterAsync(string fullName, string email, string password, UserRole role)
    {
        var response = await Http.PostAsJsonAsync("api/auth/register", new RegisterRequest
        {
            FullName = fullName,
            Email = email,
            Password = password,
            Role = role
        });
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }
}
