namespace SuperMetroid.Core.Game;

/// <summary>One compiled Nuclear Waffle projectile word at its bank-$86 address.</summary>
internal readonly record struct NuclearWaffleProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the twelve-frame articulated Nuclear Waffle/Puromi body loop.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class NuclearWaffleProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_PuromiBody</c> at $86:BB5E.</summary>
    internal const ushort Initial = 0xbb5e;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the body loop at $86:BB8E.
    /// </summary>
    internal const ushort LoopCommand = 0xbb8e;

    private static readonly NuclearWaffleProjectileInstructionMechanicsWord[] Words =
    [
        new(Initial, 0x0003),
        new(0xbb62, 0x0003),
        new(0xbb66, 0x0003),
        new(0xbb6a, 0x0003),
        new(0xbb6e, 0x0003),
        new(0xbb72, 0x0003),
        new(0xbb76, 0x0003),
        new(0xbb7a, 0x0003),
        new(0xbb7e, 0x0003),
        new(0xbb82, 0x0003),
        new(0xbb86, 0x0003),
        new(0xbb8a, 0x0003),
        new(LoopCommand, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xbb90, Initial),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xbb60, 0xbb64, 0xbb68, 0xbb6c, 0xbb70, 0xbb74,
        0xbb78, 0xbb7c, 0xbb80, 0xbb84, 0xbb88, 0xbb8c,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static NuclearWaffleProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            NuclearWaffleProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Nuclear Waffle projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
