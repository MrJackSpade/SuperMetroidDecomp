namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Immutable physical component of a Ceres steam extended frame.</summary>
/// <param name="Offset">The component's offset from the steam's position.</param>
/// <param name="Hitboxes">The component's rectangles, tested in order; empty while the plume is hidden.</param>
internal readonly record struct CeresSteamCollisionComponent(Offset16<RoomPixels> Offset, IReadOnlyList<CeresSteamCollisionHitbox> Hitboxes);
