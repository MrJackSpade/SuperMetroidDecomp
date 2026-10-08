namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Alcoon's four-frame fireball animation loop.
/// Interleaved spritemap operands are visual identities in the installed
/// enemy-projectile artwork catalog; mechanics remain compiled here.
/// </summary>
internal abstract class AlcoonFireballInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_AlcoonFireball</c> at $86:9E9E.</summary>
    internal const ushort Initial = 0x9e9e;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the fireball loop at $86:9EAE.
    /// </summary>
    internal const ushort Loop = 0x9eae;
    public static int PresentationWordCount => 4;

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + 2 + 4 * index);
    }

    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (Initial + 2);
        return (uint)offset < 16 && offset % 4 == 0;
    }
    /// <summary>Four duration/visual pairs, then goto and its loop-start operand.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - Initial;
        if ((uint)offset < 16 && offset % 4 == 0) return 3;
        if (address == Loop) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY;
        if (address == Loop + 2) return Initial;
        throw new InvalidDataException(
            $"Alcoon-fireball instruction mechanics pointer $86:{address:X4} is not compiled.");
    }
}
