namespace SuperMetroid.Core.Game;

/// <summary>One compiled Fune/Namihe fireball mechanics word at its bank-$86 address.</summary>
internal readonly record struct FuneNamiheFireballInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for both directional Fune/Namihe fireball programs. Their
/// interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal static class FuneNamiheFireballInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_NamiFuneFireball_Left</c> at $86:DE96.</summary>
    internal const ushort Left = 0xde96;

    /// <summary><c>InstList_EnemyProjectile_NamiFuneFireball_Right</c> at $86:DEA6.</summary>
    internal const ushort Right = 0xdea6;

    // Each facing has three five-tick frames followed by a jump to its start.
    internal static int MechanicsWordCount => 10;
    internal static int PresentationWordCount => 6;

    internal static FuneNamiheFireballInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int word = index % 5;
        ushort address = (ushort)(Left + 16 * (index / 5) +
            (word < 3 ? 4 * word : 12 + 2 * (word - 3)));
        return new(address, ReadMechanicsWord(address));
    }

    internal static ushort PresentationWordAddress(int index)
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
                return EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY;
            if (stage == 14)
                return (ushort)(address - 14);
        }
        throw new InvalidDataException(
            $"Fune/Namihe fireball instruction mechanics pointer $86:{address:X4} " +
            "is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = unchecked((ushort)address) - Left;
        return (uint)offset < 32 && (offset % 16 >= 12 || offset % 4 < 2);
    }
}