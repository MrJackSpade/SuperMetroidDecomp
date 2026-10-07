namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from Atomic's four directional animation loops.</summary>
/// <remarks>
/// The six durations and terminal goto in each list are immutable simulation control. The
/// interleaved visual selectors are compiled in
/// <see cref="Assets.EnemySpritemapDefinitions"/>.
/// </remarks>
internal abstract class AtomicInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>$A8:E310</c>, spinning up-right.</summary>
    internal const ushort UpRight = 0xe310;

    /// <summary><c>$A8:E32C</c>, spinning up-left.</summary>
    internal const ushort UpLeft = 0xe32c;

    /// <summary><c>$A8:E348</c>, spinning down-left.</summary>
    internal const ushort DownLeft = 0xe348;

    /// <summary><c>$A8:E364</c>, spinning down-right.</summary>
    internal const ushort DownRight = 0xe364;

    // Four loops: six duration/visual pairs followed by goto and its target.
    public static int MechanicsWordCount => 4 * 8;
    public static int PresentationWordCount => 4 * 6;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 8;
        ushort address = (ushort)(UpRight + 28 * (index / 8) +
            (word < 6 ? 4 * word : 24 + 2 * (word - 6)));
        return new(address, ReadMechanicsWord(address));
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(UpRight + 28 * (index / 6) + 4 * (index % 6) + 2);
    }

    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (UpRight + 2);
        return (uint)offset < 4 * 28 && offset % 28 <= 20 && offset % 4 == 0;
    }

    /// <summary>Evaluates the six eight-frame durations, goto and loop target at
    /// $A8:E310..E37F. Interleaved visual operands remain outside this decoder.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - UpRight;
        if ((uint)offset < 4 * 28 && (offset & 1) == 0)
        {
            int stage = offset % 28;
            if (stage < 24 && stage % 4 == 0) return 8;
            if (stage == 24) return CommonEnemyInstructionCodes.Goto;
            if (stage == 26) return (ushort)(address - 26);
        }
        throw new InvalidDataException(
            $"Atomic instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        int offset = (ushort)address - UpRight;
        if ((uint)offset >= 4 * 28) return false;
        int stage = offset % 28;
        return stage >= 24 || stage % 4 < 2;
    }
}
