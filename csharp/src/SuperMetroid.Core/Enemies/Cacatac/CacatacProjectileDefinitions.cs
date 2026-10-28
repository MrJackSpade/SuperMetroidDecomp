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

    /// <summary>The selector a spike keeps for its direction: the authored even direction byte of its spawn cue.</summary>
    internal static readonly ParameterVariants<CacatacSpikeDirection> SpikeSelectors = new(new Dictionary<int, CacatacSpikeDirection>
    {
        [0] = CacatacSpikeDirection.LeftFacingUp,
        [2] = CacatacSpikeDirection.Up,
        [4] = CacatacSpikeDirection.RightFacingUp,
        [6] = CacatacSpikeDirection.LeftFacingDown,
        [8] = CacatacSpikeDirection.Down,
        [10] = CacatacSpikeDirection.RightFacingDown,
        [12] = CacatacSpikeDirection.UpLeft,
        [14] = CacatacSpikeDirection.UpRight,
        [16] = CacatacSpikeDirection.DownLeft,
        [18] = CacatacSpikeDirection.DownRight,
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
    /// $86:D96A CacatacSpike_InstListPointers: dispatch the spike direction and
    /// facing to its corresponding named animation program.
    /// </summary>
    internal static CacatacProjectileProgram Program(CacatacSpikeDirection direction)
    {
        return direction switch
        {
            CacatacSpikeDirection.LeftFacingUp => CacatacProjectileProgram.LeftFacingUp,
            CacatacSpikeDirection.Up => CacatacProjectileProgram.Up,
            CacatacSpikeDirection.RightFacingUp => CacatacProjectileProgram.RightFacingUp,
            CacatacSpikeDirection.LeftFacingDown => CacatacProjectileProgram.LeftFacingDown,
            CacatacSpikeDirection.Down => CacatacProjectileProgram.Down,
            CacatacSpikeDirection.RightFacingDown => CacatacProjectileProgram.RightFacingDown,
            CacatacSpikeDirection.UpLeft => CacatacProjectileProgram.UpLeft,
            CacatacSpikeDirection.UpRight => CacatacProjectileProgram.UpRight,
            CacatacSpikeDirection.DownLeft => CacatacProjectileProgram.DownLeft,
            CacatacSpikeDirection.DownRight => CacatacProjectileProgram.DownRight,
            _ => throw new InvalidDataException($"Cacatac spike direction {direction} is outside the ten native selectors."),
        };
    }

    /// <summary>
    /// The native initializer compares its even direction selector to $000C
    /// at $86:D9CA. For directions $00..$0A, magnitude is $0200; for
    /// $0C..$12, magnitude is $0180. The positive word is that signed 8.8
    /// magnitude and the negative word is its 16-bit two's complement.
    /// </summary>
    internal static CacatacSpikeSpeedPair SpeedPair(CacatacSpikeDirection direction)
    {
        _ = Program(direction); // Apply the identical native direction domain.
        return direction is CacatacSpikeDirection.UpLeft or CacatacSpikeDirection.UpRight or
                CacatacSpikeDirection.DownLeft or CacatacSpikeDirection.DownRight
            ? DiagonalSpeeds
            : CardinalSpeeds;
    }
}
