namespace SuperMetroid.Core.Game;

/// <summary>One compiled Kraid-rock projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct KraidRockProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Kraid's rock projectiles and the initial Kago-bug pose they share.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class KraidRockProjectileInstructionProgramDefinitions
{
    /// <summary>
    /// <c>InstList_EnemyProjectile_KraidRocks_KagoBug</c> at $86:9C7D.
    /// </summary>
    internal const ushort SharedRockAndKagoBug = 0x9c7d;

    /// <summary>
    /// <c>InstList_EnemyProjectile_KraidFloorRocks_Right</c> at $86:9C83.
    /// </summary>
    internal const ushort RisingRockRight = 0x9c83;

    /// <summary>
    /// <c>InstList_EnemyProjectile_Shot_KraidRockSpit</c> at $86:9C89.
    /// </summary>
    internal const ushort SpitRockShot = 0x9c89;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_Sleep</c> in the shared list at $86:9C81.
    /// </summary>
    internal const ushort SharedRockAndKagoBugSleep = 0x9c81;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_Sleep</c> in the right-rock list at $86:9C87.
    /// </summary>
    internal const ushort RisingRockRightSleep = 0x9c87;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_Delete</c> ending the spit-rock shot list at $86:9CA1.
    /// </summary>
    internal const ushort SpitRockShotDelete = 0x9ca1;

    private static readonly KraidRockProjectileInstructionMechanicsWord[] Words =
    [
        new(SharedRockAndKagoBug, 0x7fff),
        new(SharedRockAndKagoBugSleep,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(RisingRockRight, 0x7fff),
        new(RisingRockRightSleep,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(SpitRockShot,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0x9c8b,
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KraidRockSpit_UsePalette0),
        new(0x9c8d, 0x0004),
        new(0x9c91, 0x0004),
        new(0x9c95, 0x0004),
        new(0x9c99, 0x0004),
        new(0x9c9d, 0x0004),
        new(SpitRockShotDelete, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x9c7f,
        0x9c85,
        0x9c8f,
        0x9c93,
        0x9c97,
        0x9c9b,
        0x9c9f,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static KraidRockProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind, ushort address) => kind switch
    {
        RoomEnemyProjectileKind.KraidSpitRock =>
            IsSharedProgramAddress(address) || IsSpitShotProgramAddress(address),
        RoomEnemyProjectileKind.KraidCeilingRock or
        RoomEnemyProjectileKind.KraidRisingRockLeft or
        RoomEnemyProjectileKind.KagoBug => IsSharedProgramAddress(address),
        RoomEnemyProjectileKind.KraidRisingRockRight => IsRisingRightProgramAddress(address),
        _ => false,
    };

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            KraidRockProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Kraid-rock projectile instruction mechanics pointer $86:{address:X4} " +
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

    private static bool IsSharedProgramAddress(ushort address) =>
        address is SharedRockAndKagoBug or SharedRockAndKagoBugSleep;

    private static bool IsRisingRightProgramAddress(ushort address) =>
        address is RisingRockRight or RisingRockRightSleep;

    private static bool IsSpitShotProgramAddress(ushort address) =>
        address >= SpitRockShot && address <= SpitRockShotDelete && (address & 1) != 0;
}
