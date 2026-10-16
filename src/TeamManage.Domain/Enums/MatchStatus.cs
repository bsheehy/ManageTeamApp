using System.Text.Json.Serialization;

namespace TeamManage.Domain.Enums;

/// <summary>
/// The lifecycle state of a scheduled match/fixture.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<MatchStatus>))]
public enum MatchStatus
{
    Scheduled = 0,
    Completed = 1,
    Cancelled = 2
}
