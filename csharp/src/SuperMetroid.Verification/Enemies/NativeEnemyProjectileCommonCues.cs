using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// How a family whose typed programs publish only <see cref="EnemyProjectileCommonCue"/>s matches its native lists: each
/// general bank-$86 command, its operand bytes and the shared cue it becomes.
/// </summary>
internal static class NativeEnemyProjectileCommonCues
{
    /// <summary>Each general command a common-cue family's lists use: its cue and operand byte count.</summary>
    private static readonly Dictionary<ushort, (EnemyProjectileCommonCueKind Kind, int OperandBytes)> Commands = new()
    {
        [(ushort)NativeEnemyProjectileInstruction.Delete] = (EnemyProjectileCommonCueKind.Delete, 0),
        [(ushort)NativeEnemyProjectileInstruction.PreInstructionInY] = (EnemyProjectileCommonCueKind.SetPreInstruction, 2),
        [(ushort)NativeEnemyProjectileInstruction.ClearPreInstruction] = (EnemyProjectileCommonCueKind.ClearPreInstruction, 0),
        [(ushort)NativeEnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius] = (EnemyProjectileCommonCueKind.Scatter, 4),
        [(ushort)NativeEnemyProjectileInstruction.TourianStatueResetDustPosition] = (EnemyProjectileCommonCueKind.ReturnToOrigin, 0),
    };

    /// <summary>The oracle rules for a common-cue family.</summary>
    internal static NativeEnemyProjectileCueRules<EnemyProjectileCommonCue> Rules { get; } = new(
        word => Commands.TryGetValue(word, out var command) ? command.OperandBytes : null,
        (word, operands, cue, at) =>
        {
            var command = Commands[word];
            AssertEqual(command.Kind, cue.Kind, $"{at}: cue");
            ushort preInstruction = command.Kind == EnemyProjectileCommonCueKind.SetPreInstruction
                ? SnesLittleEndian.Word(operands[0], operands[1])
                : (ushort)0;
            AssertEqual(preInstruction, EnemyProjectilePreInstructionAccess.Word(cue.PreInstruction), $"{at}: pre-instruction");
            // The scatter's operands pack each axis's range in the low byte and its center in the high byte.
            EnemyProjectileScatter scatter = command.Kind == EnemyProjectileCommonCueKind.Scatter
                ? new(new(operands[0], operands[2]), new(operands[1], operands[3]))
                : default;
            AssertEqual(scatter, cue.Scatter, $"{at}: scatter");
        },
        cue => cue.Kind == EnemyProjectileCommonCueKind.Delete);
}
