namespace SuperMetroid.Core.Game;

/// <summary>One compiled KiHunter-acid mechanics word at its bank-$86 address.</summary>
internal readonly record struct KiHunterAcidSpitInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for both KiHunter acid-spit introductions and their shared splash.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class KiHunterAcidSpitInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_KiHunterAcidSpit_Left</c> at $86:CF34.</summary>
    internal const ushort Left = 0xcf34;

    /// <summary><c>InstList_EnemyProjectile_KiHunterAcidSpit_HitFloor</c> at $86:CF56.</summary>
    internal const ushort HitFloor = 0xcf56;

    /// <summary><c>InstList_EnemyProjectile_KiHunterAcidSpit_Right</c> at $86:CF6E.</summary>
    internal const ushort Right = 0xcf6e;

    private static readonly KiHunterAcidSpitInstructionMechanicsWord[] Words =
    [
        new(Left, 0x0003),
        new(0xcf38, 0x0003),
        new(0xcf3c, 0x0004),
        new(0xcf40, 0x0003),
        new(0xcf44, 0x0001),
        new(0xcf48, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0xcf4a, EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Left),
        new(0xcf4c, 0x0001),
        new(0xcf50, 0x0001),
        new(0xcf54, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(HitFloor, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xcf58, 0x000c),
        new(0xcf5c, 0x000a),
        new(0xcf60, 0x000a),
        new(0xcf64, 0x0008),
        new(0xcf68, 0x0008),
        new(0xcf6c, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        new(Right, 0x0003),
        new(0xcf72, 0x0003),
        new(0xcf76, 0x0004),
        new(0xcf7a, 0x0003),
        new(0xcf7e, 0x0001),
        new(0xcf82, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0xcf84, EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Right),
        new(0xcf86, 0x0001),
        new(0xcf8a, 0x0001),
        new(0xcf8e, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xcf36, 0xcf3a, 0xcf3e, 0xcf42, 0xcf46, 0xcf4e, 0xcf52,
        0xcf5a, 0xcf5e, 0xcf62, 0xcf66, 0xcf6a,
        0xcf70, 0xcf74, 0xcf78, 0xcf7c, 0xcf80, 0xcf88, 0xcf8c,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static KiHunterAcidSpitInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.KiHunterAcidSpitLeft or
        RoomEnemyProjectileKind.KiHunterAcidSpitRight;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            KiHunterAcidSpitInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"KiHunter acid-spit instruction mechanics pointer $86:{address:X4} is not compiled.");
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
