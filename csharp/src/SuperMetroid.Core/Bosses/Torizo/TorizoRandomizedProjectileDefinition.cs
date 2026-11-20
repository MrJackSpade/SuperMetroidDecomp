namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One Torizo launch: the animation it starts, its offset from the Torizo and its velocity, whose two axes receive
/// independent signed random displacements when the projectile starts.
/// </summary>
/// <typeparam name="TSegment">The segments of the launched projectile's animation family.</typeparam>
internal readonly record struct TorizoRandomizedProjectileDefinition<TSegment>(
    TSegment Animation,
    Offset16<RoomPixels> Offset,
    PixelVelocity<RoomPixels> BaseVelocity)
    where TSegment : struct;
