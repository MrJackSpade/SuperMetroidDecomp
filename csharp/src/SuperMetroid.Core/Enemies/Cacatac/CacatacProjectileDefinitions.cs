namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Compiled program selection and launch-speed definitions for Cacatac spikes.</summary>
internal static class CacatacProjectileDefinitions
{
    /// <summary>The patrol direction each authored population parameter-one low byte selects.</summary>
    internal static readonly ParameterVariants<CacatacDirection> Directions = new(new Dictionary<int, CacatacDirection>
    {
        [0] = CacatacDirection.Left,
        [1] = CacatacDirection.Right,
        [2] = CacatacDirection.InitialStop,
    });

    /// <summary>
    /// Native immediate operands $86:D9BB/$86:D9C1: a cardinal spike moves two pixels per update
    /// (signed 8.8 raw 512) toward its negative or positive side.
    /// </summary>
    private static readonly CacatacSpikeSpeedPair CardinalSpeeds = new(
        SignedEightEight.FromSignedRaw(-512),
        SignedEightEight.FromSignedRaw(512));

    /// <summary>
    /// Native immediate operands $86:D9CF/$86:D9D5: a diagonal spike moves one and a half pixels per
    /// update on each axis (signed 8.8 raw 384) toward its negative or positive side.
    /// </summary>
    private static readonly CacatacSpikeSpeedPair DiagonalSpeeds = new(
        SignedEightEight.FromSignedRaw(-384),
        SignedEightEight.FromSignedRaw(384));

    /// <summary>
    /// $86:D96A CacatacSpike_InstListPointers: each spike direction and facing's named animation program.
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
    /// Each spike direction's launch speeds. The native initializer compares its even direction selector to $000C at
    /// $86:D9CA: directions $00..$0A are cardinal, with magnitude $0200; $0C..$12 are diagonal, with magnitude $0180.
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
