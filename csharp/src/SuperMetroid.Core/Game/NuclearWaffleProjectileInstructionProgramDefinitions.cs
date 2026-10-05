namespace SuperMetroid.Core.Game;

/// <summary>One compiled Nuclear Waffle projectile word at its bank-$86 address.</summary>
internal readonly record struct NuclearWaffleProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the twelve-frame articulated Nuclear Waffle/Puromi body loop.
/// Interleaved spritemap operands select independently installed presentation data.
/// </summary>
internal static class NuclearWaffleProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_PuromiBody</c> at $86:BB5E.</summary>
    internal const ushort Initial = 0xbb5e;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the body loop at $86:BB8E.
    /// </summary>
    internal const ushort LoopCommand = 0xbb8e;

    private const int FrameCount = 12;
    internal static int MechanicsWordCount => FrameCount + 2;
    internal static int PresentationWordCount => FrameCount;
    internal static NuclearWaffleProjectileInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(Initial + (index < FrameCount ? index * 4 : FrameCount * 4 + (index - FrameCount) * 2));
        return new(address, ReadMechanicsWord(address));
    }
    internal static ushort PresentationWordAddress(int index)
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
    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (ushort)address - Initial;
        return (uint)offset < FrameCount * 4 + 4 && (offset >= FrameCount * 4 || offset % 4 < 2);
    }
}
