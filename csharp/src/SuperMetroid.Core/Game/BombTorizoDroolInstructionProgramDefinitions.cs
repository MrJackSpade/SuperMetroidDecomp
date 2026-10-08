namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Bomb Torizo's low-health drool programs at $86:A46A-$A49D.
/// The seven interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class BombTorizoDroolInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProj_BombTorizoLowHealthDrool_4FrameDelay</c> at $86:A46A.</summary>
    internal const ushort FourFrameDelay = 0xa46a;
    /// <summary><c>InstList_EnemyProj_BombTorizoLowHealthDrool_2FrameDelay</c> at $86:A46E.</summary>
    internal const ushort TwoFrameDelay = 0xa46e;
    /// <summary><c>InstList_EnemyProj_BombTorizoLowHealthDrool_NoDelay_0</c> at $86:A472.</summary>
    internal const ushort NoDelay = 0xa472;
    /// <summary><c>InstList_EnemyProj_BombTorizoLowHealthDrool_NoDelay_1</c> at $86:A482.</summary>
    internal const ushort FallingLoop = 0xa482;
    /// <summary><c>InstList_EnemyProj_BombTorizoLowHealthDrool_HitWall</c> at $86:A48A.</summary>
    internal const ushort WallImpact = 0xa48a;
    /// <summary><c>InstList_EnemyProj_BombTorizoLowHealthDrool_HitFloor</c> at $86:A48E.</summary>
    internal const ushort FloorImpact = 0xa48e;
    /// <summary><c>PreInst_EnemyProjectile_BombTorizoLowHealthDrool_Falling</c> at $86:A887.</summary>
    internal const ushort FallingPreInstruction = 0xa887;
    public static int PresentationWordCount => 7;
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return index switch
        {
            < 2 => (ushort)(FourFrameDelay + 2 + 4 * index),
            2 => NoDelay + 10,
            3 => FallingLoop + 2,
            _ => (ushort)(FloorImpact + 4 + 4 * (index - 4)),
        };
    }
    internal static bool IsPresentationWord(ushort address)
    {
        int impact = address - (FloorImpact + 4);
        return address == FourFrameDelay + 2 || address == TwoFrameDelay + 2 ||
            address == NoDelay + 10 || address == FallingLoop + 2 ||
            ((uint)impact < 12 && impact % 4 == 0);
    }

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool or
        RoomEnemyProjectileKind.BombTorizoInitialDrool;

    /// <summary>
    /// Applies the native <c>LSR / AND #$000E</c> index into the eight-entry
    /// <c>$86:A64D</c> low-health-drool instruction-list table.
    /// </summary>
    internal static ushort SelectLowHealthInitialProgram(ushort random) =>
        (((random >> 2) & 7) % 3) switch
        {
            0 => NoDelay,
            1 => TwoFrameDelay,
            _ => FourFrameDelay,
        };

    internal static ushort ReadMechanicsWord(ushort address) => address switch
    {
        // Each blank frame adds two ticks before falling setup.
        FourFrameDelay or TwoFrameDelay => 2,
        NoDelay => EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY,
        NoDelay + 2 => FallingPreInstruction,
        NoDelay + 4 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY,
        NoDelay + 6 => 0x3000,
        NoDelay + 8 => 5,
        NoDelay + 12 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_AndY,
        NoDelay + 14 => 0xefff,
        FallingLoop => 64,
        FallingLoop + 4 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY,
        FallingLoop + 6 => FallingLoop,
        // Wall collision deletes immediately; floor collision shows three eight-tick poses.
        WallImpact or FloorImpact => EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction,
        WallImpact + 2 or FloorImpact + 14 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
        FloorImpact + 2 or FloorImpact + 6 or FloorImpact + 10 => 8,
        _ => throw new InvalidDataException($"Bomb Torizo drool mechanics pointer $86:{address:X4} is not compiled."),
    };
}
