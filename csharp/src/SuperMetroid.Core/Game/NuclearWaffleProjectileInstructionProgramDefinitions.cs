namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the twelve-frame articulated Nuclear Waffle/Puromi body loop.
/// Interleaved spritemap operands select independently installed presentation data.
/// </summary>
internal abstract class NuclearWaffleProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_PuromiBody</c> at $86:BB5E.</summary>
    internal const ushort Initial = 0xbb5e;

    /// <summary>Number of three-tick body poses before the native list jumps back to its first frame.</summary>
    internal const int FrameCount = 12;
    /// <summary>One independently installed spritemap operand for each timed pose in the body loop.</summary>
    public static int PresentationWordCount => FrameCount;
    /// <summary>Finds the bank-$86 spritemap operand following the selected pose's three-tick duration word.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + index * 4 + 2);
    }
    /// <summary>Supplies compiled three-tick durations and the final goto pair while rejecting presentation words.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - Initial;
        if ((uint)offset < FrameCount * 4 && offset % 4 == 0) return 3;
        if (offset == FrameCount * 4) return (ushort)EnemyProjectileInstruction.GotoY;
        if (offset == FrameCount * 4 + 2) return Initial;
        throw new InvalidDataException($"Nuclear Waffle projectile mechanics pointer $86:{address:X4} is not compiled.");
    }
}
