namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from Nuclear Waffle's body animation loop.</summary>
/// <remarks>
/// Frame durations and terminal loop control are immutable simulation data. The twelve
/// interleaved selections resolve compiled identities to installed head compositions.
/// </remarks>
internal abstract class NuclearWaffleInstructionProgramDefinitions
{
    /// <summary><c>$A6:9490</c>, the twelve-frame body animation loop.</summary>
    internal const ushort BodyLoop = 0x9490;

    internal const int FrameCount = 12;
    public static int PresentationWordCount => FrameCount;
    public static ushort PresentationWordAddress(int index)
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
}
