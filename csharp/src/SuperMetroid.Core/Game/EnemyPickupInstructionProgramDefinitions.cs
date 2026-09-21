namespace SuperMetroid.Core.Game;

/// <summary>One compiled enemy-pickup mechanics word at its bank-$86 address.</summary>
internal readonly record struct EnemyPickupInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the five live enemy-pickup animation programs. The sixteen
/// spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class EnemyPickupInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_Pickup_SmallEnergy</c> at $86:ED8D.</summary>
    internal const ushort SmallEnergy = 0xed8d;

    /// <summary><c>InstList_EnemyProjectile_Pickup_BigEnergy</c> at $86:EDA3.</summary>
    internal const ushort BigEnergy = 0xeda3;

    /// <summary><c>InstList_EnemyProjectile_Pickup_Missiles</c> at $86:EDB9.</summary>
    internal const ushort Missiles = 0xedb9;

    /// <summary><c>InstList_EnemyProjectile_Pickup_SuperMissiles</c> at $86:EDDD.</summary>
    internal const ushort SuperMissiles = 0xeddd;

    /// <summary><c>InstList_EnemyProjectile_Pickup_PowerBombs</c> at $86:EDEB.</summary>
    internal const ushort PowerBombs = 0xedeb;

    private static readonly EnemyPickupInstructionMechanicsWord[] Words =
    [
        new(SmallEnergy, 0x0008),
        new(0xed91, 0x0008),
        new(0xed95, 0x0008),
        new(0xed99, 0x0008),
        new(0xed9d, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xed9f, SmallEnergy),
        new(0xeda1, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(BigEnergy, 0x0008),
        new(0xeda7, 0x0008),
        new(0xedab, 0x0008),
        new(0xedaf, 0x0008),
        new(0xedb3, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xedb5, BigEnergy),
        new(0xedb7, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(Missiles, 0x0005),
        new(0xedbd, 0x0005),
        new(0xedc1, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xedc3, Missiles),
        new(0xedc5, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(SuperMissiles, 0x0005),
        new(0xede1, 0x0005),
        new(0xede5, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xede7, SuperMissiles),
        new(0xede9, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(PowerBombs, 0x0005),
        new(0xedef, 0x0005),
        new(0xedf3, 0x0005),
        new(0xedf7, 0x0005),
        new(0xedfb, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xedfd, PowerBombs),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xed8f, 0xed93, 0xed97, 0xed9b,
        0xeda5, 0xeda9, 0xedad, 0xedb1,
        0xedbb, 0xedbf,
        0xeddf, 0xede3,
        0xeded, 0xedf1, 0xedf5, 0xedf9,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static EnemyPickupInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind, ushort address) =>
        (kind is RoomEnemyProjectileKind.EnemyDeathPickup or
            RoomEnemyProjectileKind.EnemyDeathExplosion) &&
        FindWord(address) >= 0;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int index = FindWord(address);
        if (index >= 0)
            return Words[index].Value;

        throw new InvalidDataException(
            $"Enemy-pickup mechanics pointer $86:{address:X4} is not compiled.");
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

    private static int FindWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            EnemyPickupInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return middle;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        return -1;
    }
}
