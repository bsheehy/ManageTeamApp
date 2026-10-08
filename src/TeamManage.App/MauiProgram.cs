using Microsoft.Extensions.Logging;
using TeamManage.App.Configuration;
using TeamManage.App.Services;
using TeamManage.App.ViewModels;
using TeamManage.App.Views;

namespace TeamManage.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<ISessionService, SessionService>();
		builder.Services.AddSingleton<ILocalCacheService, LocalCacheService>();
		builder.Services.AddTransient<AuthHeaderHandler>();
		builder.Services.AddTransient<TokenRefreshHandler>();

		// Dedicated client for the refresh-token call itself - deliberately has no
		// handlers attached so a failed refresh can never recursively trigger another.
		builder.Services.AddHttpClient(TokenRefreshHandler.RefreshClientName, ConfigureApiClient)
					.ConfigureDevCertificateHandler();

		// TokenRefreshHandler is registered first (outermost) so it observes the final
		// response after AuthHeaderHandler (innermost) has attached the bearer token,
		// allowing it to catch 401s and retry with a freshly refreshed token.
		builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(ConfigureApiClient)
			.ConfigureDevCertificateHandler()
			.AddHttpMessageHandler<TokenRefreshHandler>()
			.AddHttpMessageHandler<AuthHeaderHandler>();
		builder.Services.AddHttpClient<ITeamApiService, TeamApiService>(ConfigureApiClient)
			.ConfigureDevCertificateHandler()
			.AddHttpMessageHandler<TokenRefreshHandler>()
			.AddHttpMessageHandler<AuthHeaderHandler>();
		builder.Services.AddHttpClient<IMatchApiService, MatchApiService>(ConfigureApiClient)
			.ConfigureDevCertificateHandler()
			.AddHttpMessageHandler<TokenRefreshHandler>()
			.AddHttpMessageHandler<AuthHeaderHandler>();

		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<TeamsViewModel>();
		builder.Services.AddTransient<TeamDetailViewModel>();
		builder.Services.AddTransient<CreateTeamViewModel>();
		builder.Services.AddTransient<CreateMatchViewModel>();
		builder.Services.AddTransient<MatchesViewModel>();
		builder.Services.AddTransient<MatchDetailViewModel>();

		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<TeamsPage>();
		builder.Services.AddTransient<TeamDetailPage>();
		builder.Services.AddTransient<CreateTeamPage>();
		builder.Services.AddTransient<CreateMatchPage>();
		builder.Services.AddTransient<MatchesPage>();
		builder.Services.AddTransient<MatchDetailPage>();

		return builder.Build();
	}

	private static void ConfigureApiClient(HttpClient client) =>
		client.BaseAddress = new Uri(ApiConfig.BaseAddress);

	/// <summary>
	/// Debug-only: lets the Android emulator trust the API's self-signed dev certificate
	/// when calling https://10.0.2.2. No-op in Release builds and on other platforms.
	/// </summary>
	private static IHttpClientBuilder ConfigureDevCertificateHandler(this IHttpClientBuilder builder)
	{
#if ANDROID && DEBUG
		builder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
				errors == System.Net.Security.SslPolicyErrors.None ||
				message.RequestUri?.Host == "10.0.2.2"
		});
#endif
		return builder;
	}
}
