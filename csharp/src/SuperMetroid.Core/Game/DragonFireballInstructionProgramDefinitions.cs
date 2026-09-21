namespace SuperMetroid.Core.Game;

/// <summary>One compiled Dragon-fireball mechanics word at its bank-$86 address.</summary>
internal readonly record struct DragonFireballInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Dragon's left/right rising and falling fireball loops.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class DragonFireballInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Rising_Left</c> at $86:B4BF.</summary>
    internal const ushort RisingLeft = 0xb4bf;

    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Rising_Right</c> at $86:B4CB.</summary>
    internal const ushort RisingRight = 0xb4cb;

    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Falling_Left</c> at $86:B4D7.</summary>
    internal const ushort FallingLeft = 0xb4d7;

    /// <summary><c>InstList_EnemyProjectile_DragonFireball_Falling_Right</c> at $86:B4E3.</summary>
    internal const ushort FallingRight = 0xb4e3;

    private static readonly DragonFireballInstructionMechanicsWord[] Words =
    [
        new(RisingLeft, 0x0005),
        new(0xb4c3, 0x0005),
        new(0xb4c7, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb4c9, RisingLeft),

        new(RisingRight, 0x0005),
        new(0xb4cf, 0x0005),
        new(0xb4d3, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb4d5, RisingRight),

        new(FallingLeft, 0x0005),
        new(0xb4db, 0x0005),
        new(0xb4df, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb4e1, FallingLeft),

        new(FallingRight, 0x0005),
        new(0xb4e7, 0x0005),
        new(0xb4eb, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb4ed, FallingRight),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xb4c1,
        0xb4c5,
        0xb4cd,
        0xb4d1,
        0xb4d9,
        0xb4dd,
        0xb4e5,
        0xb4e9,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static DragonFireballInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            DragonFireballInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Dragon-fireball instruction mechanics pointer $86:{address:X4} " +
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
