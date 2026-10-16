using System.Text.Json.Serialization;

namespace TeamManage.Domain.Enums;

/// <summary>
/// The sport a team/panel plays. Determines sensible defaults for squad size.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SportType>))]
public enum SportType
{
    Gaelic_Football = 0,
    Hurling = 1,
    Soccer = 2,
    Basketball = 3,
    Rugby = 4,
    Other = 5
}
