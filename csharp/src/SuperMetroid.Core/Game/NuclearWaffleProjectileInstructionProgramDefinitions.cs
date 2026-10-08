namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the twelve-frame articulated Nuclear Waffle/Puromi body loop.
/// Interleaved spritemap operands select independently installed presentation data.
/// </summary>
internal abstract class NuclearWaffleProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_PuromiBody</c> at $86:BB5E.</summary>
    internal const ushort Initial = 0xbb5e;

    internal const int FrameCount = 12;
    public static int PresentationWordCount => FrameCount;
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + index * 4 + 2);
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - Initial;
        if ((uint)offset < FrameCount * 4 && offset % 4 == 0) return 3;
        if (offset == FrameCount * 4) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY;
        if (offset == FrameCount * 4 + 2) return Initial;
        throw new InvalidDataException($"Nuclear Waffle projectile mechanics pointer $86:{address:X4} is not compiled.");
    }
}
