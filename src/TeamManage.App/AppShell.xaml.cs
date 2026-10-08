using Microsoft.Extensions.DependencyInjection;
using TeamManage.App.Services;
using TeamManage.App.Views;
using TeamManage.Domain.Enums;

namespace TeamManage.App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Detail/modal pages reached via relative navigation (not top-level
		// Shell sections), so they're registered here rather than as ShellContent.
		Routing.RegisterRoute("register", typeof(RegisterPage));
		Routing.RegisterRoute("teamdetail", typeof(TeamDetailPage));
		Routing.RegisterRoute("createteam", typeof(CreateTeamPage));
		Routing.RegisterRoute("creatematch", typeof(CreateMatchPage));
		Routing.RegisterRoute("matchdetail", typeof(MatchDetailPage));

		Loaded += OnLoaded;
	}

	private async void OnLoaded(object? sender, EventArgs e)
	{
		Loaded -= OnLoaded;

		var session = IPlatformApplication.Current!.Services.GetRequiredService<ISessionService>();
		await session.RestoreAsync();

		if (session.IsAuthenticated)
		{
			var destination = session.Role == UserRole.Manager ? "//teams" : "//matches";
			await GoToAsync(destination);
		}
	}
}
