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

    /// <summary>Native loop length; each frame contributes one duration and one visual selector.</summary>
    internal const int FrameCount = 12;
    /// <summary>Each of the twelve animation records retains one installed visual operand.</summary>
    public static int PresentationWordCount => FrameCount;
    /// <summary>Locates the visual selector two bytes after a frame's duration in bank $A6.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(BodyLoop + index * 4 + 2);
    }
    /// <summary>Reads the compiled three-tick durations and terminal branch, rejecting visual or unknown addresses.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - BodyLoop;
        if ((uint)offset < FrameCount * 4 && offset % 4 == 0) return 3;
        if (offset == FrameCount * 4) return (ushort)CommonEnemyInstruction.Goto;
        if (offset == FrameCount * 4 + 2) return BodyLoop;
        throw new InvalidDataException($"Nuclear Waffle instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }
}
