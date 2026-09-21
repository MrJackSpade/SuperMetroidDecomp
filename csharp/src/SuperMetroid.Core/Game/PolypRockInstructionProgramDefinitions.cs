namespace SuperMetroid.Core.Game;

/// <summary>One compiled Polyp-rock mechanics word at its bank-$86 address.</summary>
internal readonly record struct PolypRockInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Polyp's single-frame lava-rock animation.
/// Its spritemap operand remains live cartridge presentation data.
/// </summary>
internal static class PolypRockInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_NorfairLavaquakeRocks</c> at $86:BBD5.</summary>
    internal const ushort Initial = 0xbbd5;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_Sleep</c> holding the lava-rock frame at $86:BBD9.
    /// </summary>
    internal const ushort Sleep = 0xbbd9;

    private static readonly PolypRockInstructionMechanicsWord[] Words =
    [
        new(Initial, 0x0001),
        new(Sleep, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
    ];

    /// <summary>Spritemap operand at $86:BBD7.</summary>
    internal const ushort PresentationWord = 0xbbd7;

    internal static int MechanicsWordCount => Words.Length;
    internal static PolypRockInstructionMechanicsWord MechanicsWord(int index) => Words[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            PolypRockInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Polyp-rock instruction mechanics pointer $86:{address:X4} is not compiled.");
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
