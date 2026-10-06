using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoEyeBeamAttackMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// The callable Golden Torizo eye-beam attack at $AA:D10D-D17A. This list has
/// no spritemap operands: it times and spawns projectiles while four existing
/// $814B descriptors stream editable Torizo character art into VRAM.
/// </summary>
internal static class GoldenTorizoEyeBeamAttackInstructionProgramDefinitions
{
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_0</c> at $AA:D10D.</summary>
    internal const ushort Start = 0xd10d;
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_1</c> at $AA:D11F.</summary>
    internal const ushort SpawnLoop = 0xd11f;
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_2</c> at $AA:D133.</summary>
    internal const ushort TileLoop = 0xd133;
    /// <summary>The first byte after the callable list, $AA:D17B.</summary>
    internal const ushort End = 0xd17b;

    /// <summary><c>Function_GoldenTorizo_SimpleMovement</c> at $AA:D5DF.</summary>
    private const ushort TorizoSimpleMovementFunction = 0xd5df;
    /// <summary><c>Function_GoldenTorizo_NormalMovement</c> at $AA:D5E6.</summary>
    private const ushort TorizoNormalMovementFunction = 0xd5e6;

    /// <summary>Native program bank $AA.</summary>
    internal const byte Bank = 0xaa;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xd10d),
        Entry(Start),
        Op(TorizoInstructionCodes.Instruction_Torizo_FunctionInY, TorizoSimpleMovementFunction),
        Op(End),
        Op(TorizoInstructionCodes.Instruction_Torizo_SetAnimationLock),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0008),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0004),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_QueueLaserSFX),
        Entry(SpawnLoop),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnEyeBeam, 0x0000),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0004),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, SpawnLoop),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0008),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0002),
        Entry(TileLoop),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0003),
        Op(CommonEnemyInstructionCodes.CopyToVram),
        Origin(0xd140),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0003),
        Op(CommonEnemyInstructionCodes.CopyToVram),
        Origin(0xd14d),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0003),
        Op(CommonEnemyInstructionCodes.CopyToVram),
        Origin(0xd15a),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0003),
        Op(CommonEnemyInstructionCodes.CopyToVram),
        Origin(0xd167),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, TileLoop),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_EnableEyeBeamExplosions),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0008),
        Op(End),
        Op(TorizoInstructionCodes.Instruction_Torizo_ClearAnimationLock),
        Op(TorizoInstructionCodes.Instruction_Torizo_FunctionInY, TorizoNormalMovementFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_Return));

    internal static int MechanicsWordCount => Layout.MechanicsWordCount;
    internal static GoldenTorizoEyeBeamAttackMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }

    internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
        Layout.TryReadMechanicsWord(address, out value);

    internal static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
