namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Compiled program selection and launch-speed definitions for Cacatac spikes.</summary>
internal static class CacatacProjectileDefinitions
{
    /// <summary>
    /// Native immediate operands $86:D9BB/$86:D9C1: cardinal signed 8.8
    /// negative/positive speed pair ($FE00, $0200).
    /// </summary>
    private static readonly CacatacSpikeSpeedPair CardinalSpeeds = new(0xfe00, 0x0200);

    /// <summary>
    /// Native immediate operands $86:D9CF/$86:D9D5: diagonal signed 8.8
    /// negative/positive speed pair ($FE80, $0180).
    /// </summary>
    private static readonly CacatacSpikeSpeedPair DiagonalSpeeds = new(0xfe80, 0x0180);

    /// <summary>
    /// $86:D96A CacatacSpike_InstListPointers: dispatch the spike direction and
    /// facing to its corresponding named animation program.
    /// </summary>
    internal static CacatacProjectileProgram Program(CacatacSpikeDirection direction)
    {
        ushort raw = (ushort)direction;
        if ((raw & 1) != 0 || raw > (ushort)CacatacSpikeDirection.DownRight)
        {
            throw new InvalidDataException(
                $"Cacatac spike direction ${raw:X4} is outside the ten even native selectors.");
        }

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
            _ => throw new InvalidDataException(),
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
        return direction >= CacatacSpikeDirection.UpLeft
            ? DiagonalSpeeds
            : CardinalSpeeds;
    }
}
