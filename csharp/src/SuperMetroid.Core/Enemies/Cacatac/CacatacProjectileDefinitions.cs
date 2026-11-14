namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Compiled animation selection and launch-speed definitions for Cacatac spikes.</summary>
internal static class CacatacProjectileDefinitions
{

    /// <summary>
    /// A cardinal spike moves two pixels per update toward its negative or positive side.
    /// </summary>
    private static readonly CacatacSpikeSpeedPair CardinalSpeeds = new(
        -SignedEightEight.FromParts(2, 0),
        SignedEightEight.FromParts(2, 0));

    /// <summary>
    /// A diagonal spike moves one and a half pixels per
    /// update on each axis toward its negative or positive side.
    /// </summary>
    private static readonly CacatacSpikeSpeedPair DiagonalSpeeds = new(
        -SignedEightEight.FromParts(1, 128),
        SignedEightEight.FromParts(1, 128));

    /// <summary>
    /// Each spike direction and facing's named animation.
    /// </summary>
    internal static readonly Dictionary<CacatacSpikeDirection, CacatacProjectileSegment> Animations = new()
    {
        [CacatacSpikeDirection.LeftFacingUp] = CacatacProjectileSegment.LeftFacingUp,
        [CacatacSpikeDirection.Up] = CacatacProjectileSegment.Up,
        [CacatacSpikeDirection.RightFacingUp] = CacatacProjectileSegment.RightFacingUp,
        [CacatacSpikeDirection.LeftFacingDown] = CacatacProjectileSegment.LeftFacingDown,
        [CacatacSpikeDirection.Down] = CacatacProjectileSegment.Down,
        [CacatacSpikeDirection.RightFacingDown] = CacatacProjectileSegment.RightFacingDown,
        [CacatacSpikeDirection.UpLeft] = CacatacProjectileSegment.UpLeft,
        [CacatacSpikeDirection.UpRight] = CacatacProjectileSegment.UpRight,
        [CacatacSpikeDirection.DownLeft] = CacatacProjectileSegment.DownLeft,
        [CacatacSpikeDirection.DownRight] = CacatacProjectileSegment.DownRight,
    };

    /// <summary>
    /// Each spike direction's launch speeds: the sideways and straight spikes are cardinal, two pixels per update, and
    /// the four diagonal spikes move one and a half on each axis.
    /// </summary>
    internal static readonly Dictionary<CacatacSpikeDirection, CacatacSpikeSpeedPair> Speeds = new()
    {
        [CacatacSpikeDirection.LeftFacingUp] = CardinalSpeeds,
        [CacatacSpikeDirection.Up] = CardinalSpeeds,
        [CacatacSpikeDirection.RightFacingUp] = CardinalSpeeds,
        [CacatacSpikeDirection.LeftFacingDown] = CardinalSpeeds,
        [CacatacSpikeDirection.Down] = CardinalSpeeds,
        [CacatacSpikeDirection.RightFacingDown] = CardinalSpeeds,
        [CacatacSpikeDirection.UpLeft] = DiagonalSpeeds,
        [CacatacSpikeDirection.UpRight] = DiagonalSpeeds,
        [CacatacSpikeDirection.DownLeft] = DiagonalSpeeds,
        [CacatacSpikeDirection.DownRight] = DiagonalSpeeds,
    };
}
