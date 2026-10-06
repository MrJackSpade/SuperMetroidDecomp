namespace SuperMetroid.Core.Assets;

/// <summary>Mochtroid flight/attachment pose identities; independent artwork and timing stay separate.</summary>
internal static class MochtroidVisualDefinitions
{
    /// <summary>$A3: Mochtroid instruction and spritemap bank.</summary>
    internal const byte Bank = 0xa3;
    internal const int FrameCount = 6;
    /// <summary>$A3:A747: first spritemap operand in the free-flight loop at A745.</summary>
    private const ushort FlightOperand = 0xa747;
    /// <summary>$A3:A75B: first spritemap operand in the attached loop at A759.</summary>
    private const ushort AttachedOperand = 0xa75b;
    /// <summary>$A3:A9B0: first free-flight composition; the first two records each have six parts.</summary>
    private const ushort FlightFrame = 0xa9b0;
    /// <summary>$A3:AA06: first attached composition; each preceding record has four parts.</summary>
    private const ushort AttachedFrame = 0xaa06;

    internal static IEnumerable<EnemySpritemapDefinition> Frames()
    {
        for (int pose = 0; pose < 3; pose++)
            yield return new(Bank, PosePointer(false, pose), $"mochtroid_flight_{pose}");
        for (int pose = 0; pose < 3; pose++)
            yield return new(Bank, PosePointer(true, pose), $"mochtroid_attached_{pose}");
    }

    private static ushort PosePointer(bool attached, int pose) =>
        checked((ushort)((attached ? AttachedFrame : FlightFrame) + pose * (2 + 5 * (attached ? 4 : 6))));

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
