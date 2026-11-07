namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How Samus uses a map, energy or missile station: she runs into one of the access blocks either side of the station
/// toward it, and the station shows its animations while she does.
/// </summary>
/// <param name="Kind">The station kind the rule belongs to.</param>
/// <param name="RightOffset">The right access block's distance in blocks from the station block.</param>
/// <param name="LeftOffset">The left access block's distance in blocks from the station block.</param>
/// <param name="RightBehavior">The access Samus makes from the right side.</param>
/// <param name="LeftBehavior">The access Samus makes from the left side.</param>
/// <param name="Animation">The station's ordinary animation.</param>
/// <param name="AcquiredAnimation">The animation the station shows once its reward is held; null when it keeps its ordinary one.</param>
internal sealed record StationAccessRule(
    StationKind Kind,
    int RightOffset,
    int LeftOffset,
    StationAccessBehavior RightBehavior,
    StationAccessBehavior LeftBehavior,
    StationAnimationList Animation,
    StationAnimationList? AcquiredAnimation);
