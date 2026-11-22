namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One Torizo launch: the animation it starts, its offset from the Torizo and its velocity, whose two axes receive
/// independent signed random displacements when the projectile starts.
/// </summary>
internal readonly partial record struct TorizoRandomizedProjectileDefinition(
    TorizoProjectileSegment Animation,
    Offset16<RoomPixels> Offset,
    PixelVelocity<RoomPixels> BaseVelocity);
