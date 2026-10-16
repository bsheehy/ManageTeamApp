namespace TeamManage.Application.DTOs.Managers;

public class TeamManagerSummaryDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int TeamCount { get; set; }

    /// <summary>True until the manager has signed in and set their own password.</summary>
    public bool InvitePending { get; set; }

    public override string ToString() => FullName;
}

public class SaveTeamManagerRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
