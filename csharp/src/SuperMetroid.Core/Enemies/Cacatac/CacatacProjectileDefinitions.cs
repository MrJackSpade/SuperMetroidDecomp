namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Compiled program selection and launch-speed definitions for Cacatac spikes.</summary>
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
    /// Each spike direction and facing's named animation program.
    /// </summary>
    internal static readonly Dictionary<CacatacSpikeDirection, CacatacProjectileProgram> Programs = new()
    {
        [CacatacSpikeDirection.LeftFacingUp] = CacatacProjectileProgram.LeftFacingUp,
        [CacatacSpikeDirection.Up] = CacatacProjectileProgram.Up,
        [CacatacSpikeDirection.RightFacingUp] = CacatacProjectileProgram.RightFacingUp,
        [CacatacSpikeDirection.LeftFacingDown] = CacatacProjectileProgram.LeftFacingDown,
        [CacatacSpikeDirection.Down] = CacatacProjectileProgram.Down,
        [CacatacSpikeDirection.RightFacingDown] = CacatacProjectileProgram.RightFacingDown,
        [CacatacSpikeDirection.UpLeft] = CacatacProjectileProgram.UpLeft,
        [CacatacSpikeDirection.UpRight] = CacatacProjectileProgram.UpRight,
        [CacatacSpikeDirection.DownLeft] = CacatacProjectileProgram.DownLeft,
        [CacatacSpikeDirection.DownRight] = CacatacProjectileProgram.DownRight,
    };

    /// <summary>
    /// Each spike direction's launch speeds. The initializer compares its even direction selector to 12:
    /// directions 0..10 are cardinal, two pixels per update; 12..18 are diagonal, one and a half.
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
