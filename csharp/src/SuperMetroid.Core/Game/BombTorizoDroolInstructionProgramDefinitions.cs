namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Bomb Torizo's low-health drool programs at $86:A46A-$A49D.
/// The seven interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class BombTorizoDroolInstructionProgramDefinitions
{
    /// <summary>First byte of the contiguous drool stream, the 4-frame-delay list.</summary>
    internal const ushort StreamStart = (ushort)BombTorizoDroolProgram.FourFrameDelay;

    /// <summary><c>PreInst_EnemyProjectile_BombTorizoLowHealthDrool_Falling</c> at $86:A887.</summary>
    internal const ushort FallingPreInstruction = 0xa887;
    /// <summary>Seven visual selectors interleaved with the drool control stream.</summary>
    public static int PresentationWordCount => 7;
    /// <summary>Locates an extracted spritemap operand by its order across delay, falling, and impact lists.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return index switch
        {
            < 2 => (ushort)(StreamStart + 2 + 4 * index),
            2 => (ushort)BombTorizoDroolProgram.NoDelay + 10,
            3 => (ushort)BombTorizoDroolProgram.FallingLoop + 2,
            _ => (ushort)((ushort)BombTorizoDroolProgram.FloorImpact + 4 + 4 * (index - 4)),
        };
    }
    /// <summary>Distinguishes an interleaved visual selector from a compiled control word.</summary>
    internal static bool IsPresentationWord(ushort address)
    {
        int impact = address - ((ushort)BombTorizoDroolProgram.FloorImpact + 4);
        return address == (ushort)BombTorizoDroolProgram.FourFrameDelay + 2 ||
            address == (ushort)BombTorizoDroolProgram.TwoFrameDelay + 2 ||
            address == (ushort)BombTorizoDroolProgram.NoDelay + 10 ||
            address == (ushort)BombTorizoDroolProgram.FallingLoop + 2 ||
            ((uint)impact < 12 && impact % 4 == 0);
    }

    /// <summary>Recognizes the low-health and initial Bomb Torizo drool projectile kinds.</summary>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool or
        RoomEnemyProjectileKind.BombTorizoInitialDrool;

    /// <summary>
    /// Applies the native <c>LSR / AND #$000E</c> index into the eight-entry
    /// <c>$86:A64D</c> low-health-drool instruction-list table.
    /// </summary>
    internal static BombTorizoDroolProgram SelectLowHealthInitialProgram(ushort random) =>
        (((random >> 2) & 7) % 3) switch
        {
            0 => BombTorizoDroolProgram.NoDelay,
            1 => BombTorizoDroolProgram.TwoFrameDelay,
            _ => BombTorizoDroolProgram.FourFrameDelay,
        };

    /// <summary>
    /// Resolves one control word by its byte position in the contiguous $86:A46A-$A49D
    /// stream: 4-frame delay +0, 2-frame delay +4, no-delay +8, falling loop +24,
    /// wall impact +32 and floor impact +36.
    /// </summary>
    internal static ushort ReadMechanicsWord(ushort address) => (address - StreamStart) switch
    {
        // Each blank frame adds two ticks before falling setup.
        0 or 4 => 2,
        8 => (ushort)EnemyProjectileInstruction.PreInstructionInY,
        10 => FallingPreInstruction,
        12 => (ushort)EnemyProjectileInstruction.Properties_OrY,
        14 => 0x3000,
        16 => 5,
        20 => (ushort)EnemyProjectileInstruction.Properties_AndY,
        22 => 0xefff,
        24 => 64,
        28 => (ushort)EnemyProjectileInstruction.GotoY,
        30 => (ushort)BombTorizoDroolProgram.FallingLoop,
        // Wall collision deletes immediately; floor collision shows three eight-tick poses.
        32 or 36 => (ushort)EnemyProjectileInstruction.ClearPreInstruction,
        34 or 50 => (ushort)EnemyProjectileInstruction.Delete,
        38 or 42 or 46 => 8,
        _ => throw new InvalidDataException($"Bomb Torizo drool mechanics pointer $86:{address:X4} is not compiled."),
    };
}
