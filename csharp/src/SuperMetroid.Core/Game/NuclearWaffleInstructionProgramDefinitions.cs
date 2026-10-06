namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A6 address.</summary>
internal readonly record struct NuclearWaffleInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from Nuclear Waffle's body animation loop.</summary>
/// <remarks>
/// Frame durations and terminal loop control are immutable simulation data. The twelve
/// interleaved selections resolve compiled identities to installed head compositions.
/// </remarks>
internal static class NuclearWaffleInstructionProgramDefinitions
{
    /// <summary><c>$A6:9490</c>, the twelve-frame body animation loop.</summary>
    internal const ushort BodyLoop = 0x9490;

    private const int FrameCount = 12;
    internal static int MechanicsWordCount => FrameCount + 2;
    internal static int PresentationWordCount => FrameCount;
    internal static NuclearWaffleInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(BodyLoop + (index < FrameCount ? index * 4 : FrameCount * 4 + (index - FrameCount) * 2));
        return new(address, ReadMechanicsWord(address));
    }
    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(BodyLoop + index * 4 + 2);
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - BodyLoop;
        if ((uint)offset < FrameCount * 4 && offset % 4 == 0) return 3;
        if (offset == FrameCount * 4) return CommonEnemyInstructionCodes.Goto;
        if (offset == FrameCount * 4 + 2) return BodyLoop;
        throw new InvalidDataException($"Nuclear Waffle instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }
    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000) return false;
        int offset = (ushort)address - BodyLoop;
        return (uint)offset < FrameCount * 4 + 4 && (offset >= FrameCount * 4 || offset % 4 < 2);
    }
}
