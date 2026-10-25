namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Where an access station's side blocks sit and what they do, and the animations the station shows.</summary>
/// <param name="Kind">The station kind the layout belongs to.</param>
/// <param name="RightOffset">The right access block's offset from the station block.</param>
/// <param name="LeftOffset">The left access block's offset from the station block.</param>
/// <param name="RightBehavior">The right access block's behaviour.</param>
/// <param name="LeftBehavior">The left access block's behaviour.</param>
/// <param name="Animation">The station's ordinary animation.</param>
/// <param name="AcquiredAnimation">The animation the station shows once its reward is held; null when it keeps its ordinary one.</param>
internal sealed record StationAccessLayout(
    StationKind Kind,
    int RightOffset,
    int LeftOffset,
    StationAccessBehavior RightBehavior,
    StationAccessBehavior LeftBehavior,
    StationAnimationList Animation,
    StationAnimationList? AcquiredAnimation);
