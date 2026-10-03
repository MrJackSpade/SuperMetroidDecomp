namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A6 address.</summary>
internal readonly record struct BoulderInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from Boulder's mirrored rolling programs.</summary>
/// <remarks>
/// Frame durations and loop control are immutable simulation data. The sixteen
/// interleaved visual selectors are compiled in
/// <see cref="Assets.EnemySpritemapDefinitions"/>.
/// </remarks>
internal static class BoulderInstructionProgramDefinitions
{
    /// <summary><c>$A6:86A7</c>, the eight-frame left-moving rolling loop.</summary>
    internal const ushort Left = 0x86a7;

    /// <summary><c>$A6:86CB</c>, the eight-frame right-moving rolling loop.</summary>
    internal const ushort Right = 0x86cb;

    internal static int MechanicsWordCount => 20;
    internal static int PresentationWordCount => 16;

    internal static BoulderInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 10;
        ushort address = (ushort)(Left + 36 * (index / 10) + (word < 8 ? 4 * word : 32 + 2 * (word - 8)));
        return new(address, ReadMechanicsWord(address));
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Left + 36 * (index / 8) + 4 * (index % 8) + 2);
    }

    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (Left + 2);
        return (uint)offset < 72 && offset % 36 < 32 && offset % 4 == 0;
    }

    /// <summary>Eight eight-frame duration/visual pairs then goto and loop-start operand,
    /// repeated for opposite rotation. Only aligned native control starts are accepted.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - Left;
        if ((uint)offset < 72 && (offset & 1) == 0)
        {
            int stage = offset % 36;
            if (stage < 32 && stage % 4 == 0) return 8;
            if (stage == 32) return CommonEnemyInstructionCodes.Goto;
            if (stage == 34) return (ushort)(address - 34);
        }
        throw new InvalidDataException(
            $"Boulder instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000) return false;
        int offset = (ushort)address - Left;
        if ((uint)offset >= 72) return false;
        int stage = offset % 36;
        return stage >= 32 || stage % 4 < 2;
    }
}
