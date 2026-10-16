namespace TeamManage.Domain.Entities;

/// <summary>
/// A club owned by a Club Manager. Teams are created within a club, and the
/// club manager assigns a Team Manager to each team.
/// </summary>
public class Club
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string Name { get; set; } = string.Empty;

    public Guid ClubManagerId { get; set; }

    public User? ClubManager { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Team> Teams { get; set; } = new List<Team>();
}
