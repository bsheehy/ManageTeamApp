using Microsoft.Extensions.DependencyInjection;
using TeamManage.Application.Interfaces.Services;
using TeamManage.Application.Services;

namespace TeamManage.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IMatchService, MatchService>();

        return services;
    }
}
