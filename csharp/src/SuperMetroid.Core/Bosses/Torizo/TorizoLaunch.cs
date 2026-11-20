namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One of the Torizos' randomized launches, written once for a launch to the right (#2411): a launch to the left starts its
/// own animation from the mirrored offset at the mirrored velocity.
/// </summary>
/// <param name="MovingLeft">The animation a launch to the left starts.</param>
/// <param name="MovingRight">The animation a launch to the right starts.</param>
/// <param name="Offset">A launch to the right's offset from the Torizo.</param>
/// <param name="BaseVelocity">A launch to the right's velocity, before its random displacements.</param>
internal sealed record TorizoLaunch(
    TorizoProjectileSegment MovingLeft, TorizoProjectileSegment MovingRight, Offset16<RoomPixels> Offset, PixelVelocity<RoomPixels> BaseVelocity)
{
    /// <summary>The launch toward <paramref name="direction"/>.</summary>
    internal TorizoRandomizedProjectileDefinition Toward(HorizontalDirection direction) => direction == HorizontalDirection.Right
        ? new(MovingRight, Offset, BaseVelocity)
        : new(MovingLeft, Offset.MirroredX(), BaseVelocity.ReversedX());
}
