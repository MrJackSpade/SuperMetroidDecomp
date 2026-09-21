namespace SuperMetroid.Core.Game;

/// <summary>One compiled Eye Door projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct EyeDoorProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the Eye Door's aimed projectile, wall-impact animation, and shot
/// animation. Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class EyeDoorProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_EyeDoorProjectile</c> at $86:B5D9.</summary>
    internal const ushort Initial = 0xb5d9;

    /// <summary>Loop entered by the aimed projectile after setup at $86:B5EB.</summary>
    internal const ushort FlyingLoop = 0xb5eb;

    /// <summary><c>InstList_EnemyProjectile_EyeDoorProjectile_Impact</c> at $86:B5F3.</summary>
    internal const ushort Impact = 0xb5f3;

    /// <summary><c>InstList_EnemyProjectile_Shot_EyeDoorProjectile</c> at $86:B603.</summary>
    internal const ushort Shot = 0xb603;

    private static readonly EyeDoorProjectileInstructionMechanicsWord[] Words =
    [
        new(Initial, 0x0004),
        new(0xb5dd, 0x0003),
        new(0xb5e1, 0x0002),
        new(0xb5e5,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_CalculateDirectionTowardsSamus),
        new(0xb5e7, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0xb5e9, EyeDoorEnemyProjectileRomData.ProjectilePreInstruction),
        new(FlyingLoop, 0x0010),
        new(0xb5ef, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xb5f1, FlyingLoop),

        new(Impact, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xb5f5, 0x0002),
        new(0xb5f9, 0x0003),
        new(0xb5fd, 0x0004),
        new(0xb601, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(Shot, 0x0004),
        new(0xb607, 0x0004),
        new(0xb60b, 0x0004),
        new(0xb60f, 0x0004),
        new(0xb613, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xb5db,
        0xb5df,
        0xb5e3,
        0xb5ed,
        0xb5f7,
        0xb5fb,
        0xb5ff,
        0xb605,
        0xb609,
        0xb60d,
        0xb611,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static EyeDoorProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            EyeDoorProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Eye Door projectile instruction mechanics pointer $86:{address:X4} " +
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
