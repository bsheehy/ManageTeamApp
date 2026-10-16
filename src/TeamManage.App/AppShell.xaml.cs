using Microsoft.Extensions.DependencyInjection;
using TeamManage.App.Services;
using TeamManage.App.Views;

namespace TeamManage.App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Detail/modal pages reached via relative navigation (not top-level
		// Shell sections), so they're registered here rather than as ShellContent.
		Routing.RegisterRoute("register", typeof(RegisterPage));
		Routing.RegisterRoute("teams", typeof(TeamsPage));
		Routing.RegisterRoute("teamdetail", typeof(TeamDetailPage));
		Routing.RegisterRoute("createteam", typeof(CreateTeamPage));
		Routing.RegisterRoute("clubs", typeof(ClubsPage));
		Routing.RegisterRoute("managers", typeof(ManagersPage));
		Routing.RegisterRoute("createclub", typeof(CreateClubPage));
		Routing.RegisterRoute("changepassword", typeof(ChangePasswordPage));
		Routing.RegisterRoute("matches", typeof(MatchesPage));
		Routing.RegisterRoute("creatematch", typeof(CreateMatchPage));
		Routing.RegisterRoute("matchdetail", typeof(MatchDetailPage));

		Loaded += OnLoaded;
	}

	private async void OnLoaded(object? sender, EventArgs e)
	{
		Loaded -= OnLoaded;

		var session = IPlatformApplication.Current!.Services.GetRequiredService<ISessionService>();
		await session.RestoreAsync();

		if (InviteLinkHandler.HasPending)
		{
			await InviteLinkHandler.ProcessPendingAsync();
			return;
		}

		if (session.IsAuthenticated)
		{
			await GoToAsync("//dashboard");
		}
	}
}
