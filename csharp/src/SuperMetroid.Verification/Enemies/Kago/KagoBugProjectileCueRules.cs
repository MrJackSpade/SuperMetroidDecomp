using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Kago;

/// <summary>How the Kago bugs' typed programs match their native lists: each command and the cue it becomes.</summary>
internal static class KagoBugProjectileCueRules
{
    /// <summary>Each command the bugs' lists use and the cue it becomes; none carries an operand.</summary>
    private static readonly Dictionary<EnemyProjectileInstruction, KagoBugProjectileCueKind> Commands = new()
    {
        [EnemyProjectileInstruction.Delete] = KagoBugProjectileCueKind.Delete,
        [EnemyProjectileInstruction.KagoBugStartJumpInstruction] = KagoBugProjectileCueKind.StartJump,
        [EnemyProjectileInstruction.KagoBugStartIdleInstruction] = KagoBugProjectileCueKind.StartIdle,
        [EnemyProjectileInstruction.KagoBugSpawnDropInstruction] = KagoBugProjectileCueKind.SpawnDrop,
        [EnemyProjectileInstruction.KagoBugUsePaletteZeroInstruction] = KagoBugProjectileCueKind.UsePaletteZero,
    };

    /// <summary>The oracle rules for the bugs' programs.</summary>
    internal static NativeEnemyProjectileCueRules<KagoBugProjectileCue> Rules { get; } = new(
        word => Commands.ContainsKey((EnemyProjectileInstruction)word) ? 0 : null,
        (word, operands, cue, at) => AssertEqual(Commands[(EnemyProjectileInstruction)word], cue.Kind, $"{at}: cue"),
        cue => cue.Kind == KagoBugProjectileCueKind.Delete);
}
