using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>How the n00b tube's typed programs match its native lists: each command and the cue it becomes.</summary>
internal static class NoobTubeProjectileCueRules
{
    /// <summary>Each command the lists use, the cue it becomes and its operand byte count.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, (NoobTubeProjectileCueKind Kind, int OperandBytes)> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = (NoobTubeProjectileCueKind.Delete, 0),
        [NativeEnemyProjectileInstruction.PreInstructionInY] = (NoobTubeProjectileCueKind.SetPreInstruction, 2),
        [NativeEnemyProjectileInstruction.NoobTubeShardAssignFallingAngle] = (NoobTubeProjectileCueKind.AssignShardFallingAngle, 0),
        [NativeEnemyProjectileInstruction.NoobTubeBubbleAssignFallingAngle] = (NoobTubeProjectileCueKind.AssignBubbleFallingAngle, 0),
        [NativeEnemyProjectileInstruction.NoobTubeShardReflectFlicker] = (NoobTubeProjectileCueKind.FlickerReflected, 4),
        [NativeEnemyProjectileInstruction.NoobTubeShardFlicker] = (NoobTubeProjectileCueKind.FlickerHidden, 2),
    };

    /// <summary>The oracle rules for the n00b tube's programs; both flickers show their own frame.</summary>
    internal static NativeEnemyProjectileCueRules<NoobTubeProjectileCue> Rules { get; } = new(
        word => Commands.TryGetValue((NativeEnemyProjectileInstruction)word, out var command) ? command.OperandBytes : null,
        (word, operands, cue, at) =>
        {
            AssertEqual(Commands[(NativeEnemyProjectileInstruction)word].Kind, cue.Kind, $"{at}: cue");
            if (cue.Kind == NoobTubeProjectileCueKind.SetPreInstruction)
                AssertEqual(SnesLittleEndian.Word(operands[0], operands[1]), EnemyProjectilePreInstructionAccess.Word(cue.PreInstruction), $"{at}: pre-instruction");
        },
        cue => cue.Kind == NoobTubeProjectileCueKind.Delete,
        word => (NativeEnemyProjectileInstruction)word is NativeEnemyProjectileInstruction.NoobTubeShardReflectFlicker or NativeEnemyProjectileInstruction.NoobTubeShardFlicker);
}
