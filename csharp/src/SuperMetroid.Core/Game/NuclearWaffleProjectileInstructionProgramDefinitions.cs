namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the twelve-frame articulated Nuclear Waffle/Puromi body loop.
/// Interleaved spritemap operands select independently installed presentation data.
/// </summary>
internal abstract class NuclearWaffleProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_PuromiBody</c> at $86:BB5E.</summary>
    internal const ushort Initial = 0xbb5e;

    /// <summary>Number of articulated body frames in the compiled Puromi projectile loop.</summary>
    internal const int FrameCount = 12;

    /// <summary>Number of spritemap operands retained as independently installed presentation data.</summary>
    public static int PresentationWordCount => FrameCount;

    /// <summary>Returns the address of a frame's interleaved spritemap operand in the bank-$86 instruction list.</summary>
    /// <param name="index">Zero-based frame index in the twelve-frame body loop.</param>
    /// <returns>Address of the presentation word associated with that frame.</returns>
    /// <exception cref="IndexOutOfRangeException">The frame index is outside the compiled loop.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + index * 4 + 2);
    }

    /// <summary>Resolves a body-loop address to its three-tick hold or the loop's terminal jump instruction.</summary>
    /// <param name="address">Bank-$86 address of a mechanics word in the compiled projectile list.</param>
    /// <returns>The instruction value at that mechanics address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - Initial;
        if ((uint)offset < FrameCount * 4 && offset % 4 == 0) return 3;
        if (offset == FrameCount * 4) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY;
        if (offset == FrameCount * 4 + 2) return Initial;
        throw new InvalidDataException($"Nuclear Waffle projectile mechanics pointer $86:{address:X4} is not compiled.");
    }
}
