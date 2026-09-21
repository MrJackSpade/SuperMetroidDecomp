namespace SuperMetroid.Core.Game;

internal readonly record struct BombTorizoDroolInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Bomb Torizo's low-health drool programs at $86:A46A-$A49D.
/// The seven interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class BombTorizoDroolInstructionProgramDefinitions
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

    private static readonly ushort[] InitialPrograms =
    [
        NoDelay,
        TwoFrameDelay,
        FourFrameDelay,
        NoDelay,
        TwoFrameDelay,
        FourFrameDelay,
        NoDelay,
        TwoFrameDelay,
    ];

    private static readonly BombTorizoDroolInstructionMechanicsWord[] Words =
    [
        new(FourFrameDelay, 2),
        new(TwoFrameDelay, 2),
        new(NoDelay, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0xa474, FallingPreInstruction),
        new(0xa476, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY),
        new(0xa478, 0x3000),
        new(0xa47a, 5),
        new(0xa47e, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_AndY),
        new(0xa480, 0xefff),
        new(FallingLoop, 0x0040),
        new(0xa486, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xa488, FallingLoop),
        new(WallImpact, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xa48c, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        new(FloorImpact, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0xa490, 8),
        new(0xa494, 8),
        new(0xa498, 8),
        new(0xa49c, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xa46c, 0xa470, 0xa47c, 0xa484, 0xa492, 0xa496, 0xa49a,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static BombTorizoDroolInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool or
        RoomEnemyProjectileKind.BombTorizoInitialDrool;

    /// <summary>
    /// Applies the native <c>LSR / AND #$000E</c> index into the eight-entry
    /// <c>$86:A64D</c> low-health-drool instruction-list table.
    /// </summary>
    internal static ushort SelectLowHealthInitialProgram(ushort random) =>
        InitialPrograms[(random >> 2) & 7];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            BombTorizoDroolInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Bomb Torizo drool mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (BombTorizoDroolInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
