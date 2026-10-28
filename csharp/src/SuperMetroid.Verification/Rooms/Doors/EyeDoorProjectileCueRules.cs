using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Enemies.EyeDoor;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>How the eye-door shot's typed programs match its native lists: each command and the cue it becomes.</summary>
internal static class EyeDoorProjectileCueRules
{
    /// <summary>Each command the shot's lists use, the cue it becomes and its operand byte count.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, EyeDoorProjectileCueRulesCommand> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = new EyeDoorProjectileCueRulesCommand(EyeDoorProjectileCueKind.Delete, 0),
        [NativeEnemyProjectileInstruction.ClearPreInstruction] = new EyeDoorProjectileCueRulesCommand(EyeDoorProjectileCueKind.ClearPreInstruction, 0),
        [NativeEnemyProjectileInstruction.PreInstructionInY] = new EyeDoorProjectileCueRulesCommand(EyeDoorProjectileCueKind.SetPreInstruction, 2),
        [NativeEnemyProjectileInstruction.CalculateDirectionTowardsSamus] = new EyeDoorProjectileCueRulesCommand(EyeDoorProjectileCueKind.AimAtSamus, 0),
    };

    /// <summary>The oracle rules for the shot's programs.</summary>
    internal static NativeEnemyProjectileCueRules<EyeDoorProjectileCue> Rules { get; } = new(
        word => Commands.TryGetValue((NativeEnemyProjectileInstruction)word, out var command) ? command.OperandBytes : null,
        (word, operands, cue, at) =>
        {
            AssertEqual(Commands[(NativeEnemyProjectileInstruction)word].Kind, cue.Kind, $"{at}: cue");
            if (cue.Kind == EyeDoorProjectileCueKind.SetPreInstruction)
                AssertEqual(LittleEndianBytes.Word(operands[0], operands[1]), EnemyProjectilePreInstructionAccess.Word(cue.PreInstruction), $"{at}: pre-instruction");
        },
        cue => cue.Kind == EyeDoorProjectileCueKind.Delete);
}
