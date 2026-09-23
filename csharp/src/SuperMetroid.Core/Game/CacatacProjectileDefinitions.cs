namespace SuperMetroid.Core.Game;

/// <summary>Compiled program selection and launch-speed definitions for Cacatac spikes.</summary>
internal static class CacatacProjectileDefinitions
{
    /// <summary>
    /// Cacatac spike instruction-list pointers at $86:D96A: ten word entries
    /// selected by the even <see cref="CacatacSpikeDirection"/> byte offset.
    /// For selector index j = direction / 2 in 0..9, the physical six-byte
    /// program index is {0, 2, 4, 5, 7, 9, 1, 3, 6, 8}[j] and its pointer is
    /// $D92E + 6*index. The first six selectors are cardinal directions;
    /// the final four are diagonals. Odd or out-of-range selectors are rejected.
    /// The selected programs share the compiled control definitions while retaining live
    /// cartridge spritemap operands.
    /// </summary>
    private static ReadOnlySpan<ushort> InstructionLists =>
    [
        CacatacProjectileInstructionProgramDefinitions.LeftFacingUp,
        CacatacProjectileInstructionProgramDefinitions.Up,
        CacatacProjectileInstructionProgramDefinitions.RightFacingUp,
        CacatacProjectileInstructionProgramDefinitions.LeftFacingDown,
        CacatacProjectileInstructionProgramDefinitions.Down,
        CacatacProjectileInstructionProgramDefinitions.RightFacingDown,
        CacatacProjectileInstructionProgramDefinitions.UpLeft,
        CacatacProjectileInstructionProgramDefinitions.UpRight,
        CacatacProjectileInstructionProgramDefinitions.DownLeft,
        CacatacProjectileInstructionProgramDefinitions.DownRight,
    ];

    /// <summary>$86:D9AD/$86:D9C4: cardinal signed 8.8 speed pair.</summary>
    private static readonly CacatacSpikeSpeedPair CardinalSpeeds = new(0xfe00, 0x0200);

    /// <summary>$86:D9B7/$86:D9CD: diagonal signed 8.8 speed pair.</summary>
    private static readonly CacatacSpikeSpeedPair DiagonalSpeeds = new(0xfe80, 0x0180);

    internal static ushort InstructionList(CacatacSpikeDirection direction)
    {
        ushort raw = (ushort)direction;
        if ((raw & 1) != 0 || raw > (ushort)CacatacSpikeDirection.DownRight)
        {
            throw new InvalidDataException(
                $"Cacatac spike direction ${raw:X4} is outside the ten even native selectors.");
        }

        return InstructionLists[raw >> 1];
    }

    internal static CacatacSpikeSpeedPair SpeedPair(CacatacSpikeDirection direction)
    {
        _ = InstructionList(direction); // Apply the identical native direction domain.
        return direction >= CacatacSpikeDirection.UpLeft
            ? DiagonalSpeeds
            : CardinalSpeeds;
    }
}

internal readonly record struct CacatacSpikeSpeedPair(
    ushort Negative,
    ushort Positive);
