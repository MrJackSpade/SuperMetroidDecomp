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

    /// <summary>The number of frames in the body loop, each with one presentation selector.</summary>
    internal const int FrameCount = 12;

    /// <summary>Gets the number of head-composition selectors interleaved with the body-loop frames.</summary>
    public static int PresentationWordCount => FrameCount;

    /// <summary>Gets the native address of a frame's head-composition selector.</summary>
    /// <param name="index">The zero-based frame and selector index.</param>
    /// <returns>The address of that frame's presentation word in bank $A6.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the twelve-frame body loop.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(BodyLoop + index * 4 + 2);
    }
    /// <summary>Resolves a compiled body-loop mechanics word from its native address.</summary>
    /// <param name="address">The bank-$A6 address of a word in the body animation program.</param>
    /// <returns>The frame duration, loop opcode, or loop destination encoded at that address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - BodyLoop;
        if ((uint)offset < FrameCount * 4 && offset % 4 == 0) return 3;
        if (offset == FrameCount * 4) return CommonEnemyInstructionCodes.Goto;
        if (offset == FrameCount * 4 + 2) return BodyLoop;
        throw new InvalidDataException($"Nuclear Waffle instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }
}
