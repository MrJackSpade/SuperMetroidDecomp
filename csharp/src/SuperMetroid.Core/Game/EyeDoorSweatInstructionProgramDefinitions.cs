namespace SuperMetroid.Core.Game;

/// <summary>One compiled Eye Door sweat mechanics word at its bank-$86 address.</summary>
internal readonly record struct EyeDoorSweatInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for an Eye Door sweat drop's falling loop and floor-impact animation.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class EyeDoorSweatInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_EyeDoorSweat</c> at $86:B615.</summary>
    internal const ushort Initial = 0xb615;

    /// <summary><c>InstList_EnemyProjectile_EyeDoorSweat_Impact</c> at $86:B61D.</summary>
    internal const ushort Impact = 0xb61d;

    private static readonly EyeDoorSweatInstructionMechanicsWord[] Words =
    [
        new(Initial, 0x0006),
        new(0xb619, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb61b, Initial),

        new(Impact, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xb61f, 0x0006),
        new(0xb623, 0x0006),
        new(0xb627, 0x0006),
        new(0xb62b, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xb617,
        0xb621,
        0xb625,
        0xb629,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static EyeDoorSweatInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            EyeDoorSweatInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Eye Door sweat instruction mechanics pointer $86:{address:X4} " +
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
