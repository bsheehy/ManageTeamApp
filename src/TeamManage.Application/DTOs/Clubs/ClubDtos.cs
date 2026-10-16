namespace TeamManage.Application.DTOs.Clubs;

public class ClubDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int TeamCount { get; set; }

    public override string ToString() => Name;
}

public class CreateClubRequest
{
    public string Name { get; set; } = string.Empty;
}
