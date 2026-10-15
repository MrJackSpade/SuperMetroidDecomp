using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>How the n00b tube's typed programs match its native lists: each command and the cue it becomes.</summary>
internal static class NoobTubeProjectileCueRules
{
    /// <summary>Each command the lists use, the cue it becomes and its operand byte count.</summary>
    private static readonly Dictionary<EnemyProjectileInstruction, (NoobTubeProjectileCueKind Kind, int OperandBytes)> Commands = new()
    {
        [EnemyProjectileInstruction.Delete] = (NoobTubeProjectileCueKind.Delete, 0),
        [EnemyProjectileInstruction.PreInstructionInY] = (NoobTubeProjectileCueKind.SetPreInstruction, 2),
        [EnemyProjectileInstruction.NoobTubeShardAssignFallingAngle] = (NoobTubeProjectileCueKind.AssignShardFallingAngle, 0),
        [EnemyProjectileInstruction.NoobTubeBubbleAssignFallingAngle] = (NoobTubeProjectileCueKind.AssignBubbleFallingAngle, 0),
        [EnemyProjectileInstruction.NoobTubeShardReflectFlicker] = (NoobTubeProjectileCueKind.FlickerReflected, 4),
        [EnemyProjectileInstruction.NoobTubeShardFlicker] = (NoobTubeProjectileCueKind.FlickerHidden, 2),
    };

    /// <summary>The oracle rules for the n00b tube's programs; both flickers show their own frame.</summary>
    internal static NativeEnemyProjectileCueRules<NoobTubeProjectileCue> Rules { get; } = new(
        word => Commands.TryGetValue((EnemyProjectileInstruction)word, out var command) ? command.OperandBytes : null,
        (word, operands, cue, at) =>
        {
            AssertEqual(Commands[(EnemyProjectileInstruction)word].Kind, cue.Kind, $"{at}: cue");
            if (cue.Kind == NoobTubeProjectileCueKind.SetPreInstruction)
                AssertEqual(SnesLittleEndian.Word(operands[0], operands[1]), (ushort)cue.PreInstruction, $"{at}: pre-instruction");
        },
        cue => cue.Kind == NoobTubeProjectileCueKind.Delete,
        word => (EnemyProjectileInstruction)word is EnemyProjectileInstruction.NoobTubeShardReflectFlicker or EnemyProjectileInstruction.NoobTubeShardFlicker);
}
