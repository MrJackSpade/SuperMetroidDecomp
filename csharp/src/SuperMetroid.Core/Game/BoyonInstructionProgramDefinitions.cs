namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from Boyon's idle and bouncing programs.</summary>
/// <remarks>
/// Property commands, callbacks, durations, and loop control are immutable simulation
/// data. The ten interleaved visual selectors are compiled separately in
/// <see cref="Assets.EnemySpritemapDefinitions"/>; neither belongs in editable artwork.
/// </remarks>
internal abstract class BoyonInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>$A2:86A7</c>, the four-frame idle loop.</summary>
    internal const ushort Idle = 0x86a7;

    /// <summary><c>$A2:86BF</c>, the six-frame bouncing loop.</summary>
    internal const ushort Bouncing = 0x86bf;

    public static int MechanicsWordCount => 18;
    public static int PresentationWordCount => 10;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool bouncing = index >= 8;
        int word = bouncing ? index - 8 : index;
        int frames = bouncing ? 6 : 4;
        int offset = word < 2 ? 2 * word : word < frames + 2
            ? 4 + 4 * (word - 2) : 4 + 4 * frames + 2 * (word - frames - 2);
        ushort address = (ushort)((bouncing ? Bouncing : Idle) + offset);
        return new(address, ReadMechanicsWord(address));
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 4 ? Idle + 6 + 4 * index : Bouncing + 6 + 4 * (index - 4));
    }

    internal static bool IsPresentationWord(ushort address)
    {
        bool bouncing = address >= Bouncing;
        int offset = address - (bouncing ? Bouncing : Idle) - 6;
        return (uint)offset < (bouncing ? 24u : 16u) && offset % 4 == 0;
    }

    /// <summary>Setup property/callback, timed animation, then goto the first timed frame.
    /// The idle loop has four ten-frame steps; bouncing has six five-frame steps.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        bool bouncing = address >= Bouncing;
        ushort start = bouncing ? Bouncing : Idle;
        int offset = address - start;
        int frames = bouncing ? 6 : 4;
        if (offset == 0) return bouncing ? CommonEnemyInstructionCodes.EnableOffScreenProcessing
            : CommonEnemyInstructionCodes.DisableOffScreenProcessing;
        if (offset == 2) return bouncing ? EnemyInstructionCodePointers.Instruction_Boyon_88C6
            : EnemyInstructionCodePointers.RTL_A288C5;
        if (offset >= 4 && offset < 4 + 4 * frames && offset % 4 == 0)
            return (ushort)(bouncing ? 5 : 10);
        if (offset == 4 + 4 * frames) return CommonEnemyInstructionCodes.Goto;
        if (offset == 6 + 4 * frames) return (ushort)(start + 4);
        throw new InvalidDataException(
            $"Boyon instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = (ushort)address - Idle;
        if ((uint)offset >= 56) return false;
        bool bouncing = offset >= 24;
        int local = bouncing ? offset - 24 : offset;
        int tail = bouncing ? 28 : 20;
        return local < 4 || local >= tail || local % 4 < 2;
    }
}
