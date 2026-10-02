namespace SuperMetroid.Core.Game;

/// <summary>One compiled Alcoon-fireball mechanics word at its bank-$86 address.</summary>
internal readonly record struct AlcoonFireballInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Alcoon's four-frame fireball animation loop.
/// Interleaved spritemap operands are visual identities in the installed
/// enemy-projectile artwork catalog; mechanics remain compiled here.
/// </summary>
internal static class AlcoonFireballInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_AlcoonFireball</c> at $86:9E9E.</summary>
    internal const ushort Initial = 0x9e9e;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the fireball loop at $86:9EAE.
    /// </summary>
    internal const ushort Loop = 0x9eae;

    internal static int MechanicsWordCount => 6;
    internal static int PresentationWordCount => 4;

    internal static AlcoonFireballInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(Initial + (index < 4 ? 4 * index : 16 + 2 * (index - 4)));
        return new(address, ReadMechanicsWord(address));
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + 2 + 4 * index);
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

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (ushort)address - Initial;
        return (uint)offset < 20 && (offset >= 16 || offset % 4 < 2);
    }
}
