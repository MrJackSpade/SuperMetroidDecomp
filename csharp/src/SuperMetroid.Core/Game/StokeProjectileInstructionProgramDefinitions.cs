namespace SuperMetroid.Core.Game;

/// <summary>One compiled Stoke-projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct StokeProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the two-frame Stoke projectile animation loop.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class StokeProjectileInstructionProgramDefinitions
{
    /// <summary><c>UNUSED_InstList_EnemyProjectile_StokeProjectile_86DB0B</c> at $86:DB0C.</summary>
    internal const ushort Initial = 0xdb0c;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the projectile loop at $86:DB14.
    /// </summary>
    internal const ushort LoopCommand = 0xdb14;

    private static readonly StokeProjectileInstructionMechanicsWord[] Words =
    [
        new(Initial, 0x0010),
        new(0xdb10, 0x0010),
        new(LoopCommand, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xdb16, Initial),
    ];

    private static readonly ushort[] PresentationWords = [0xdb0e, 0xdb12];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static StokeProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            StokeProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Stoke-projectile instruction mechanics pointer $86:{address:X4} is not compiled.");
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
