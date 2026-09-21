namespace SuperMetroid.Core.Game;

/// <summary>One compiled Magdollite-lava mechanics word at its bank-$86 address.</summary>
internal readonly record struct MagdolliteLavaInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Magdollite's left/right thrown-lava poses and shot program.
/// Interleaved spritemap operands remain live cartridge presentation data; the shot
/// program's final target belongs to the shared projectile-program catalog.
/// </summary>
internal static class MagdolliteLavaInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_MagdolliteFlame_Left</c> at $86:DFD8.</summary>
    internal const ushort Left = 0xdfd8;

    /// <summary><c>InstList_EnemyProjectile_MagdolliteFlame_Right</c> at $86:DFDE.</summary>
    internal const ushort Right = 0xdfde;

    /// <summary><c>InstList_EnemyProjectile_Shot_MagdolliteFlame</c> at $86:DFE4.</summary>
    internal const ushort Shot = 0xdfe4;

    private static readonly MagdolliteLavaInstructionMechanicsWord[] Words =
    [
        new(Left, 0x0001),
        new(0xdfdc, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(Right, 0x0001),
        new(0xdfe2, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(Shot,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_MagdolliteFlame_SpawnDrops),
        new(0xdfe6, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xdfe8, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xdfda,
        0xdfe0,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static MagdolliteLavaInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            MagdolliteLavaInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Magdollite-lava instruction mechanics pointer $86:{address:X4} " +
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
