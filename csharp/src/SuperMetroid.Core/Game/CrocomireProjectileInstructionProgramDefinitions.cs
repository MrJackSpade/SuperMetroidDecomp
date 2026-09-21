namespace SuperMetroid.Core.Game;

/// <summary>One compiled Crocomire projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct CrocomireProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Crocomire's mouth projectile, bridge fragments, and spike-wall
/// pieces. Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class CrocomireProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_CrocomiresProjectile</c> at $86:8FCF.</summary>
    internal const ushort MouthProjectile = 0x8fcf;

    /// <summary>
    /// <c>InstList_EnemyProjectile_CrocomireBridgeCrumbling</c> at $86:8FEB.
    /// </summary>
    internal const ushort BridgeFragment = 0x8feb;

    /// <summary>
    /// <c>InstList_EnemyProjectile_CrocomireSpikeWallPieces</c> at $86:8FF3.
    /// </summary>
    internal const ushort SpikeWallPiece = 0x8ff3;

    /// <summary>
    /// <c>InstList_EnemyProjectile_Shot_CrocomiresProjectile</c> at $86:9007.
    /// </summary>
    internal const ushort MouthProjectileShot = 0x9007;

    private static readonly CrocomireProjectileInstructionMechanicsWord[] Words =
    [
        new(MouthProjectile, 0x0003),
        new(0x8fd3, 0x0003),
        new(0x8fd7, 0x0003),
        new(0x8fdb, 0x0003),
        new(0x8fdf, 0x0003),
        new(0x8fe3, 0x0003),
        new(0x8fe7, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x8fe9, MouthProjectile),
        new(BridgeFragment, 0x7fff),
        new(0x8fef, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x8ff1, BridgeFragment),
        new(SpikeWallPiece, 0x7fff),
        new(0x8ff7, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x8ff9, SpikeWallPiece),
        new(MouthProjectileShot, 0x0004),
        new(0x900b, 0x0004),
        new(0x900f, 0x0004),
        new(0x9013, 0x0004),
        new(0x9017, 0x0004),
        new(0x901b,
            EnemyProjectileCodePointers.Instruction_SpawnEnemyDropsWithCrocomireChances),
        new(0x901d, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x901f, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x8fd1, 0x8fd5, 0x8fd9, 0x8fdd, 0x8fe1, 0x8fe5,
        0x8fed, 0x8ff5,
        0x9009, 0x900d, 0x9011, 0x9015, 0x9019,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static CrocomireProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.CrocomireProjectile or
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling or
        RoomEnemyProjectileKind.CrocomireSpikeWallPieces;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CrocomireProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Crocomire projectile mechanics pointer $86:{address:X4} is not compiled.");
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
