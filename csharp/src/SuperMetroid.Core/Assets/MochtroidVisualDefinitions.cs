namespace SuperMetroid.Core.Assets;

/// <summary>Mochtroid flight/attachment pose identities; independent artwork and timing stay separate.</summary>
internal static class MochtroidVisualDefinitions
{
    /// <summary>$A3: Mochtroid instruction and spritemap bank.</summary>
    internal const byte Bank = 0xa3;
    /// <summary>$A3:A747: first spritemap operand in the free-flight loop at A745.</summary>
    private const ushort FlightOperand = 0xa747;
    /// <summary>$A3:A75B: first spritemap operand in the attached loop at A759.</summary>
    private const ushort AttachedOperand = 0xa75b;
    /// <summary>$A3:A9B0: first free-flight composition; the first two records each have six parts.</summary>
    private const ushort FlightFrame = 0xa9b0;
    /// <summary>$A3:AA06: first attached composition; each preceding record has four parts.</summary>
    private const ushort AttachedFrame = 0xaa06;

    /// <summary>Enumerates the three flight and three attached compositions in native pose order.</summary>
    internal static IEnumerable<EnemySpritemapDefinition> Frames()
    {
        for (int pose = 0; pose < 3; pose++)
            yield return new(Bank, PosePointer(false, pose), $"mochtroid_flight_{pose}");
        for (int pose = 0; pose < 3; pose++)
            yield return new(Bank, PosePointer(true, pose), $"mochtroid_attached_{pose}");
    }

    /// <summary>Calculates a pose's bank-relative frame pointer from its family and ordinal.</summary>
    /// <param name="attached">Selects the attached composition family instead of free flight.</param>
    /// <param name="pose">Zero-based pose ordinal within the selected family.</param>
    /// <returns>Pointer to the first word of the selected spritemap.</returns>
    private static ushort PosePointer(bool attached, int pose) =>
        checked((ushort)((attached ? AttachedFrame : FlightFrame) + pose * (2 + 5 * (attached ? 4 : 6))));

    /// <summary>Maps a compiled loop operand address to the composition selected at that phase.</summary>
    /// <param name="operandAddress">Bank-relative address of a flight or attached spritemap operand.</param>
    /// <returns>Pointer to the frame selected by that operand in the loop's forward-and-return sequence.</returns>
    /// <exception cref="InvalidDataException">The operand address is not part of either compiled loop.</exception>
    internal static ushort FrameAt(ushort operandAddress)
    {
        int offset = operandAddress - FlightOperand;
        bool attached = false;
        if (offset < 0 || offset > 12 || (offset & 3) != 0)
        {
            offset = operandAddress - AttachedOperand;
            attached = true;
            if (offset < 0 || offset > 12 || (offset & 3) != 0)
                throw new InvalidDataException($"Mochtroid visual selector $A3:{operandAddress:X4} is not compiled.");
        }
        // Each loop expands through three poses, returns through the middle, then repeats.
        int phase = offset / 4;
        return PosePointer(attached, 2 - Math.Abs(2 - phase));
    }
}
