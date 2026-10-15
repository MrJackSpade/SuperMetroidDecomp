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
    private static readonly Dictionary<EnemyProjectileInstruction, (EnemyProjectileCommonCueKind Kind, int OperandBytes)> Commands = new()
    {
        [EnemyProjectileInstruction.Delete] = (EnemyProjectileCommonCueKind.Delete, 0),
        [EnemyProjectileInstruction.PreInstructionInY] = (EnemyProjectileCommonCueKind.SetPreInstruction, 2),
        [EnemyProjectileInstruction.ClearPreInstruction] = (EnemyProjectileCommonCueKind.ClearPreInstruction, 0),
    };

    /// <summary>The oracle rules for a common-cue family.</summary>
    internal static NativeEnemyProjectileCueRules<EnemyProjectileCommonCue> Rules { get; } = new(
        word => Commands.TryGetValue((EnemyProjectileInstruction)word, out var command) ? command.OperandBytes : null,
        (word, operands, cue, at) =>
        {
            var command = Commands[(EnemyProjectileInstruction)word];
            AssertEqual(command.Kind, cue.Kind, $"{at}: cue");
            ushort preInstruction = command.Kind == EnemyProjectileCommonCueKind.SetPreInstruction
                ? SnesLittleEndian.Word(operands[0], operands[1])
                : (ushort)0;
            AssertEqual(preInstruction, (ushort)cue.PreInstruction, $"{at}: pre-instruction");
        },
        cue => cue.Kind == EnemyProjectileCommonCueKind.Delete);
}
