namespace SuperMetroid.Core.Game;

internal readonly record struct TorizoChozoOrbInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Bomb and Golden Torizo's Chozo-orb programs at $86:AB15-$AB89.
/// Their eighteen interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class TorizoChozoOrbInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_TorizoChozoOrbs_Left</c> at $86:AB15.</summary>
    internal const ushort MovingLeft = 0xab15;
    /// <summary><c>InstList_EnemyProjectile_TorizoChozoOrbs_Right</c> at $86:AB1D.</summary>
    internal const ushort MovingRight = 0xab1d;
    /// <summary><c>InstList_EnemyProjectile_TorizoChozoOrbs_BreakOnWall</c> at $86:AB25.</summary>
    internal const ushort WallImpact = 0xab25;
    /// <summary><c>InstList_EnemyProjectile_TorizoChozoOrbs_BreakOnFloor</c> at $86:AB41.</summary>
    internal const ushort FloorImpact = 0xab41;
    /// <summary><c>InstList_EnemyProjectile_Shot_TorizoChozoOrbs</c> at $86:AB68.</summary>
    internal const ushort Shot = 0xab68;
    /// <summary><c>EnemyHeaders_BombTorizoOrb</c> at $A0:EF3F.</summary>
    internal const ushort BombOrbEnemyHeader = 0xef3f;
    /// <summary><c>EnemyHeaders_GoldenTorizoOrb</c> at $A0:EFBF.</summary>
    internal const ushort GoldenOrbEnemyHeader = 0xefbf;

    private static readonly TorizoChozoOrbInstructionMechanicsWord[] Words =
    [
        new(MovingLeft, 0x0055),
        new(0xab19, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xab1b, MovingLeft),
        new(MovingRight, 0x0055),
        new(0xab21, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xab23, MovingRight),

        new(WallImpact, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xab27, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY),
        new(0xab29, 0x5000),
        new(0xab2b, 4),
        new(0xab2f, 4),
        new(0xab33, 4),
        new(0xab37, 4),
        new(0xab3b, 4),
        new(0xab3f, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(FloorImpact, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xab43, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_AndY),
        new(0xab45, 0xdfff),
        new(0xab47, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY),
        new(0xab49, 0x5000),
        new(0xab4b, EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib3_Max6),
        new(0xab4e, 4),
        new(0xab52, 5),
        new(0xab56, 6),
        new(0xab5a, 7),
        new(0xab5e, 8),
        new(0xab62, 9),
        new(0xab66, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(Shot, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xab6a, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY),
        new(0xab6c, 0x5000),
        new(0xab6e, 4),
        new(0xab72, 4),
        new(0xab76, 4),
        new(0xab7a, 4),
        new(0xab7e, 4),
        new(0xab82, EnemyProjectileCodePointers.Instruction_EnemyProjectile_SpawnEnemyDropsWIthYDropChances),
        new(0xab84, BombOrbEnemyHeader),
        new(0xab86, GoldenOrbEnemyHeader),
        new(0xab88, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xab17, 0xab1f,
        0xab2d, 0xab31, 0xab35, 0xab39, 0xab3d,
        0xab50, 0xab54, 0xab58, 0xab5c, 0xab60, 0xab64,
        0xab70, 0xab74, 0xab78, 0xab7c, 0xab80,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static TorizoChozoOrbInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoChozoOrb or
        RoomEnemyProjectileKind.GoldenTorizoChozoOrb;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            TorizoChozoOrbInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Torizo Chozo-orb mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (TorizoChozoOrbInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
