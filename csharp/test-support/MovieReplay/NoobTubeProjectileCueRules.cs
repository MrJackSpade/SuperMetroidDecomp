using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies.NoobTube;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>How the n00b tube's typed programs match its native lists: each command and the cue it becomes.</summary>
internal static class NoobTubeProjectileCueRules
{
    /// <summary>Each command the lists use, the cue it becomes and its operand byte count.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, NoobTubeProjectileCueRulesCommand> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = new NoobTubeProjectileCueRulesCommand(NoobTubeProjectileCueKind.Delete, 0),
        [NativeEnemyProjectileInstruction.FrameBehaviorInY] = new NoobTubeProjectileCueRulesCommand(null, 2),
        [NativeEnemyProjectileInstruction.NoobTubeShardAssignFallingAngle] = new NoobTubeProjectileCueRulesCommand(NoobTubeProjectileCueKind.AssignShardFallingAngle, 0),
        [NativeEnemyProjectileInstruction.NoobTubeBubbleAssignFallingAngle] = new NoobTubeProjectileCueRulesCommand(NoobTubeProjectileCueKind.AssignBubbleFallingAngle, 0),
        [NativeEnemyProjectileInstruction.NoobTubeShardReflectFlicker] = new NoobTubeProjectileCueRulesCommand(NoobTubeProjectileCueKind.FlickerReflected, 4),
        [NativeEnemyProjectileInstruction.NoobTubeShardFlicker] = new NoobTubeProjectileCueRulesCommand(NoobTubeProjectileCueKind.FlickerHidden, 2),
    };

    /// <summary>The motion each cue that starts one starts, which the native list names by its routine.</summary>
    private static readonly Dictionary<NoobTubeProjectileCueKind, NoobTubePieceMotion> StartedMotion = new()
    {
        [NoobTubeProjectileCueKind.StartCrackFlicker] = NoobTubePieceMotion.CrackFlickering,
        [NoobTubeProjectileCueKind.StartCrackFall] = NoobTubePieceMotion.CrackFalling,
        [NoobTubeProjectileCueKind.StartShardFall] = NoobTubePieceMotion.ShardFalling,
        [NoobTubeProjectileCueKind.StartBubbleRise] = NoobTubePieceMotion.BubbleFlying,
        [NoobTubeProjectileCueKind.StartBubbleFall] = NoobTubePieceMotion.BubbleFalling,
    };

    /// <summary>The oracle rules for the n00b tube's programs; both flickers show their own frame.</summary>
    internal static NativeEnemyProjectileCueRules<NoobTubeProjectileCue> Rules { get; } = new(
        word => Commands.TryGetValue((NativeEnemyProjectileInstruction)word, out var command) ? command.OperandBytes : null,
        (word, operands, cue, at) =>
        {
            if (Commands[(NativeEnemyProjectileInstruction)word].Kind is { } kind)
                VerificationAssertions.AssertEqual(kind, cue.Kind, $"{at}: cue");
            else
                // Setting a routine is the cue that starts the motion that routine stands for.
                VerificationAssertions.AssertEqual<Enum?>(StartedMotion.TryGetValue(cue.Kind, out NoobTubePieceMotion started) ? started : null,
                    EnemyProjectileMotionAccess.FromWord(LittleEndianBytes.Word(operands[0], operands[1])), $"{at}: pre-instruction");
        },
        cue => cue.Kind == NoobTubeProjectileCueKind.Delete,
        word => (NativeEnemyProjectileInstruction)word is NativeEnemyProjectileInstruction.NoobTubeShardReflectFlicker or NativeEnemyProjectileInstruction.NoobTubeShardFlicker);
}
