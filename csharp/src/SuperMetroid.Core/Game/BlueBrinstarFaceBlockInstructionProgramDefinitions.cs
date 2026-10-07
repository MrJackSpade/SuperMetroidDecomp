namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from all three Blue Brinstar face-block programs.</summary>
/// <remarks>
/// Frame durations and terminal sleeps are immutable simulation data. The seven
/// interleaved spritemap pointers select separately installed presentation data.
/// </remarks>
internal abstract class BlueBrinstarFaceBlockInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>$A8:E80C</c>, the three-frame animation when Samus approaches from the left.</summary>
    internal const ushort SamusLeft = 0xe80c;

    /// <summary><c>$A8:E81A</c>, the three-frame animation when Samus approaches from the right.</summary>
    internal const ushort SamusRight = 0xe81a;

    /// <summary><c>$A8:E828</c>, the one-frame neutral program installed at initialization.</summary>
    internal const ushort Initial = 0xe828;

    public static int MechanicsWordCount => 10;
    public static int PresentationWordCount => 7;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = index < 8 ? (ushort)(SamusLeft + 14 * (index / 4) + 4 * (index % 4))
            : (ushort)(Initial + 4 * (index - 8));
        return new(address, ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return index < 6 ? (ushort)(SamusLeft + 14 * (index / 3) + 4 * (index % 3) + 2)
            : (ushort)(Initial + 2);
    }
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - SamusLeft;
        return (uint)offset < 28 ? offset % 14 % 4 == 2 : address == Initial + 2;
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - SamusLeft;
        if ((uint)offset < 28 && offset % 14 % 4 == 0)
        {
            int local = offset % 14;
            // Wait facing forward, show the intermediate and final turn poses, then sleep.
            return local == 12 ? CommonEnemyInstructionCodes.Sleep : (ushort)(local == 0 ? 48 : 16);
        }
        if (address == Initial) return 1;
        if (address == Initial + 4) return CommonEnemyInstructionCodes.Sleep;
        throw new InvalidDataException($"Blue Brinstar face-block instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        int offset = unchecked((ushort)address) - SamusLeft;
        if ((uint)offset < 28) return offset % 14 % 4 < 2;
        int initialOffset = unchecked((ushort)address) - Initial;
        return (uint)initialOffset < 6 && initialOffset % 4 < 2;
    }
}
