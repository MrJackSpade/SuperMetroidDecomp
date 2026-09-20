namespace SuperMetroid.Core.Game;

/// <summary>Fixed Phantoon rain placements and shot-response markers.</summary>
public static class PhantoonPatternDefinitions
{
    /// <summary>
    /// <c>$A7:D40D-$D41E</c>, the nine eye-direction instruction lists selected from the
    /// computed Samus octant. Direction five is unreachable but remains authored data.
    /// </summary>
    private static readonly ushort[] EyeInstructions =
    [
        PhantoonInstructionProgramDefinitions.EyeLookingUp,
        PhantoonInstructionProgramDefinitions.EyeLookingUpRight,
        PhantoonInstructionProgramDefinitions.EyeLookingRight,
        PhantoonInstructionProgramDefinitions.EyeLookingDownRight,
        PhantoonInstructionProgramDefinitions.EyeLookingDown,
        PhantoonInstructionProgramDefinitions.EyeLookingDown,
        PhantoonInstructionProgramDefinitions.EyeLookingDownLeft,
        PhantoonInstructionProgramDefinitions.EyeLookingLeft,
        PhantoonInstructionProgramDefinitions.EyeLookingUpLeft,
    ];

    /// <summary>$A7:CDAD, Phantoon_FlameRain_PositionTable: figure-eight cursor and world X/Y; each native record also has an unused zero word.</summary>
    public static (ushort Cursor, ushort X, ushort Y) RainPlacement(int pattern) => pattern switch
    {
        0 or 4 => (1, 128, 96),
        1 => (71, 168, 64),
        2 => (136, 208, 96),
        3 => (201, 168, 128),
        5 => (334, 88, 64),
        6 => (399, 48, 96),
        7 => (465, 88, 128),
        _ => throw new ArgumentOutOfRangeException(nameof(pattern)),
    };

    /// <summary>$A7:CFC2, first flame positions: starting column before eight successive columns wrap modulo nine.</summary>
    public static ReadOnlySpan<byte> FirstRainColumns => [5, 7, 0, 7, 5, 3, 1, 3];

    /// <summary>$A7:CDA5, Phantoon_Unknown0FEAValues: shot writes to eye variable B. No native reader is known; preserve the exact exposed state without inventing direction semantics.</summary>
    public static ReadOnlySpan<byte> ShotEyeMarkers => [6, 6, 8, 8, 6, 8, 6, 8];

    /// <summary>Returns the authored eye instruction list for direction zero through eight.</summary>
    public static ushort EyeInstruction(ushort direction)
    {
        if (direction >= EyeInstructions.Length)
        {
            throw new InvalidDataException(
                $"Phantoon eye direction {direction} exceeds nine authored selectors.");
        }

        return EyeInstructions[direction];
    }
}
