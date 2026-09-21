namespace SuperMetroid.Core.Game;

/// <summary>One compiled Fune/Namihe fireball mechanics word at its bank-$86 address.</summary>
internal readonly record struct FuneNamiheFireballInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for both directional Fune/Namihe fireball programs. Their
/// interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class FuneNamiheFireballInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_NamiFuneFireball_Left</c> at $86:DE96.</summary>
    internal const ushort Left = 0xde96;

    /// <summary><c>InstList_EnemyProjectile_NamiFuneFireball_Right</c> at $86:DEA6.</summary>
    internal const ushort Right = 0xdea6;

    private static readonly FuneNamiheFireballInstructionMechanicsWord[] Words =
    [
        new(0xde96, 0x0005),
        new(0xde9a, 0x0005),
        new(0xde9e, 0x0005),
        new(0xdea2, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xdea4, Left),
        new(0xdea6, 0x0005),
        new(0xdeaa, 0x0005),
        new(0xdeae, 0x0005),
        new(0xdeb2, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xdeb4, Right),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xde98,
        0xde9c,
        0xdea0,
        0xdea8,
        0xdeac,
        0xdeb0,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static FuneNamiheFireballInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            FuneNamiheFireballInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Fune/Namihe fireball instruction mechanics pointer $86:{address:X4} " +
            "is not compiled.");
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
