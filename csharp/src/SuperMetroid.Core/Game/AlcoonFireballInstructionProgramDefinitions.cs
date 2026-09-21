namespace SuperMetroid.Core.Game;

/// <summary>One compiled Alcoon-fireball mechanics word at its bank-$86 address.</summary>
internal readonly record struct AlcoonFireballInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Alcoon's four-frame fireball animation loop.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class AlcoonFireballInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_AlcoonFireball</c> at $86:9E9E.</summary>
    internal const ushort Initial = 0x9e9e;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the fireball loop at $86:9EAE.
    /// </summary>
    internal const ushort Loop = 0x9eae;

    private static readonly AlcoonFireballInstructionMechanicsWord[] Words =
    [
        new(Initial, 0x0003),
        new(0x9ea2, 0x0003),
        new(0x9ea6, 0x0003),
        new(0x9eaa, 0x0003),
        new(Loop, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x9eb0, Initial),
    ];

    private static readonly ushort[] PresentationWords =
        [0x9ea0, 0x9ea4, 0x9ea8, 0x9eac];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static AlcoonFireballInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            AlcoonFireballInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Alcoon-fireball instruction mechanics pointer $86:{address:X4} is not compiled.");
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
