using System.Text.Json.Serialization;

namespace TeamManage.Domain.Enums;

/// <summary>
/// A player's response to a match invitation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AttendanceStatus>))]
public enum AttendanceStatus
{
    Pending = 0,
    Confirmed = 1,
    Declined = 2
}
