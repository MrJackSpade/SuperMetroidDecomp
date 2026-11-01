namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Compiled program selection and launch-speed definitions for Cacatac spikes.</summary>
internal static class CacatacProjectileDefinitions
{

    /// <summary>
    /// A cardinal spike moves two pixels per update
    /// (signed 8.8 raw 512) toward its negative or positive side.
    /// </summary>
    private static readonly CacatacSpikeSpeedPair CardinalSpeeds = new(
        SignedEightEight.FromSignedRaw(-512),
        SignedEightEight.FromSignedRaw(512));

    /// <summary>
    /// A diagonal spike moves one and a half pixels per
    /// update on each axis (signed 8.8 raw 384) toward its negative or positive side.
    /// </summary>
    private static readonly CacatacSpikeSpeedPair DiagonalSpeeds = new(
        SignedEightEight.FromSignedRaw(-384),
        SignedEightEight.FromSignedRaw(384));

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
    /// directions 0..10 are cardinal, with magnitude 512; 12..18 are diagonal, with magnitude 384.
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
