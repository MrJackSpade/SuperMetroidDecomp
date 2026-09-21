namespace SuperMetroid.Core.Game;

/// <summary>One compiled Yapping Maw body-projectile word at its bank-$86 address.</summary>
internal readonly record struct YappingMawBodyProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing and terminal sleep control for the two Yapping Maw body-link poses.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class YappingMawBodyProjectileInstructionProgramDefinitions
{
    /// <summary>
    /// <c>InstList_EnemyProjectile_YappingMawsBody_FacingDown</c> at $86:EC56.
    /// </summary>
    internal const ushort FacingDown = 0xec56;

    /// <summary>
    /// <c>InstList_EnemyProjectile_YappingMawsBody_FacingUp</c> at $86:EC5C.
    /// </summary>
    internal const ushort FacingUp = 0xec5c;

    private static readonly YappingMawBodyProjectileInstructionMechanicsWord[] Words =
    [
        new(FacingDown, 0x0001),
        new(0xec5a, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(FacingUp, 0x0001),
        new(0xec60, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
    ];

    private static readonly ushort[] PresentationWords = [0xec58, 0xec5e];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static YappingMawBodyProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            YappingMawBodyProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Yapping Maw body-projectile mechanics pointer $86:{address:X4} is not compiled.");
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
