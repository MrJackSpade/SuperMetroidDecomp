namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// One piece of Crocomire's crumbling spike wall (#2539): how its sideways acceleration grows, its fastest sideways speed,
/// where it stands in the wall and the crumble it sounds as it lands.
/// </summary>
/// <param name="AccelerationGrowth">How much the piece's sideways acceleration grows each update, in pixels per update per update.</param>
/// <param name="FastestSpeed">The piece's fastest sideways speed, in whole pixels an update.</param>
/// <param name="Depth">How far below the wall's top the piece stands as the wall breaks, in pixels.</param>
/// <param name="CrumbleSound">The crumble the piece queues as it lands, before its big explosion; none for a piece that lands without one.</param>
internal sealed record CrocomireSpikeWallPiece(SignedSixteenSixteen AccelerationGrowth, int FastestSpeed, int Depth, SoundEffectId? CrumbleSound)
{
    /// <summary>
    /// The piece's greatest sideways acceleration, in pixels per update per update:
    /// <see cref="CrocomireSpikeMotionDefinitions.MaximumAccelerationPerIncrement"/> times its growth.
    /// </summary>
    internal SignedSixteenSixteen MaximumAcceleration => AccelerationGrowth * CrocomireSpikeMotionDefinitions.MaximumAccelerationPerIncrement;
}
