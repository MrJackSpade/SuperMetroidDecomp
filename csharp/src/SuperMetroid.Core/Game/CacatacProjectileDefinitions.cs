namespace SuperMetroid.Core.Game;

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
    internal static ushort InstructionList(CacatacSpikeDirection direction)
    {
        ushort raw = (ushort)direction;
        if ((raw & 1) != 0 || raw > (ushort)CacatacSpikeDirection.DownRight)
        {
            throw new InvalidDataException(
                $"Cacatac spike direction ${raw:X4} is outside the ten even native selectors.");
        }

        return direction switch
        {
            CacatacSpikeDirection.LeftFacingUp => CacatacProjectileInstructionProgramDefinitions.LeftFacingUp,
            CacatacSpikeDirection.Up => CacatacProjectileInstructionProgramDefinitions.Up,
            CacatacSpikeDirection.RightFacingUp => CacatacProjectileInstructionProgramDefinitions.RightFacingUp,
            CacatacSpikeDirection.LeftFacingDown => CacatacProjectileInstructionProgramDefinitions.LeftFacingDown,
            CacatacSpikeDirection.Down => CacatacProjectileInstructionProgramDefinitions.Down,
            CacatacSpikeDirection.RightFacingDown => CacatacProjectileInstructionProgramDefinitions.RightFacingDown,
            CacatacSpikeDirection.UpLeft => CacatacProjectileInstructionProgramDefinitions.UpLeft,
            CacatacSpikeDirection.UpRight => CacatacProjectileInstructionProgramDefinitions.UpRight,
            CacatacSpikeDirection.DownLeft => CacatacProjectileInstructionProgramDefinitions.DownLeft,
            CacatacSpikeDirection.DownRight => CacatacProjectileInstructionProgramDefinitions.DownRight,
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
        _ = InstructionList(direction); // Apply the identical native direction domain.
        return direction >= CacatacSpikeDirection.UpLeft
            ? DiagonalSpeeds
            : CardinalSpeeds;
    }
}

/// <summary>Signed 8.8 launch-speed words selected for a Cacatac spike's direction.</summary>
/// <param name="Negative">The 16-bit two's-complement word for the negative velocity component.</param>
/// <param name="Positive">The 16-bit word for the positive velocity component.</param>
internal readonly record struct CacatacSpikeSpeedPair(
    ushort Negative,
    ushort Positive);
