using System.Text.Json.Serialization;

namespace TeamManage.Domain.Enums;

/// <summary>
/// The role a user has within the system.
/// </summary>
/// <remarks>
/// Annotated to always serialize/deserialize as its string name (e.g. "Manager"),
/// regardless of the JsonSerializerOptions used by a particular call site. Without
/// this, clients using default System.Text.Json options (which expect enums as
/// numbers) would fail to deserialize responses from the API, which serializes
/// this enum as a string via its own JsonStringEnumConverter configuration.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<UserRole>))]
public enum UserRole
{
    ClubManager = 0,
    TeamManager = 1,
    Player = 2
}
