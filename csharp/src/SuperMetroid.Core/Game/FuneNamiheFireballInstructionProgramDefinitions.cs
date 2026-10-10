namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for both directional Fune/Namihe fireball programs. Their
/// interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class FuneNamiheFireballInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_NamiFuneFireball_Left</c> at $86:DE96.</summary>
    internal const ushort Left = 0xde96;

    /// <summary><c>InstList_EnemyProjectile_NamiFuneFireball_Right</c> at $86:DEA6.</summary>
    internal const ushort Right = 0xdea6;
    public static int PresentationWordCount => 6;

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(Left + 16 * (index / 3) + 4 * (index % 3) + 2);
    }

    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (Left + 2);
        return (uint)offset < 32 && offset % 16 < 12 && offset % 4 == 0;
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - Left;
        if ((uint)offset < 32)
        {
            int stage = offset % 16;
            if (stage < 12 && stage % 4 == 0)
                return 5;
            if (stage == 12)
                return (ushort)EnemyProjectileInstruction.GotoY;
            if (stage == 14)
                return (ushort)(address - 14);
        }
        throw new InvalidDataException(
            $"Fune/Namihe fireball instruction mechanics pointer $86:{address:X4} " +
            "is not compiled.");
    }
}