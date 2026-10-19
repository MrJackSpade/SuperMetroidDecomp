using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>How the eye-door shot's typed programs match its native lists: each command and the cue it becomes.</summary>
internal static class EyeDoorProjectileCueRules
{
    /// <summary>Each command the shot's lists use, the cue it becomes and its operand byte count.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, (EyeDoorProjectileCueKind Kind, int OperandBytes)> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = (EyeDoorProjectileCueKind.Delete, 0),
        [NativeEnemyProjectileInstruction.ClearPreInstruction] = (EyeDoorProjectileCueKind.ClearPreInstruction, 0),
        [NativeEnemyProjectileInstruction.PreInstructionInY] = (EyeDoorProjectileCueKind.SetPreInstruction, 2),
        [NativeEnemyProjectileInstruction.CalculateDirectionTowardsSamus] = (EyeDoorProjectileCueKind.AimAtSamus, 0),
    };

    /// <summary>The oracle rules for the shot's programs.</summary>
    internal static NativeEnemyProjectileCueRules<EyeDoorProjectileCue> Rules { get; } = new(
        word => Commands.TryGetValue((NativeEnemyProjectileInstruction)word, out var command) ? command.OperandBytes : null,
        (word, operands, cue, at) =>
        {
            AssertEqual(Commands[(NativeEnemyProjectileInstruction)word].Kind, cue.Kind, $"{at}: cue");
            if (cue.Kind == EyeDoorProjectileCueKind.SetPreInstruction)
                AssertEqual(SnesLittleEndian.Word(operands[0], operands[1]), EnemyProjectilePreInstructionAccess.Word(cue.PreInstruction), $"{at}: pre-instruction");
        },
        cue => cue.Kind == EyeDoorProjectileCueKind.Delete);
}
