namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Where an access station's side blocks sit and what they do, and whether it has a separate acquired animation.</summary>
/// <param name="Kind">The station kind the layout belongs to.</param>
/// <param name="RightOffset">The right access block's offset from the station block.</param>
/// <param name="LeftOffset">The left access block's offset from the station block.</param>
/// <param name="RightBehavior">The right access block's behaviour.</param>
/// <param name="LeftBehavior">The left access block's behaviour.</param>
/// <param name="HasAcquiredAnimation">Whether the station shows a separate animation once its reward is held.</param>
internal sealed record StationAccessLayout(
    StationKind Kind,
    int RightOffset,
    int LeftOffset,
    StationAccessBehavior RightBehavior,
    StationAccessBehavior LeftBehavior,
    bool HasAcquiredAnimation);
