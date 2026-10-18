using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Kago;

/// <summary>How the Kago bugs' typed programs match their native lists: each command and the cue it becomes.</summary>
internal static class KagoBugProjectileCueRules
{
    /// <summary>Each command the bugs' lists use and the cue it becomes; none carries an operand.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, KagoBugProjectileCueKind> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = KagoBugProjectileCueKind.Delete,
        [NativeEnemyProjectileInstruction.KagoBugStartJumpInstruction] = KagoBugProjectileCueKind.StartJump,
        [NativeEnemyProjectileInstruction.KagoBugStartIdleInstruction] = KagoBugProjectileCueKind.StartIdle,
        [NativeEnemyProjectileInstruction.KagoBugSpawnDropInstruction] = KagoBugProjectileCueKind.SpawnDrop,
        [NativeEnemyProjectileInstruction.KagoBugUsePaletteZeroInstruction] = KagoBugProjectileCueKind.UsePaletteZero,
    };

    /// <summary>The oracle rules for the bugs' programs.</summary>
    internal static NativeEnemyProjectileCueRules<KagoBugProjectileCue> Rules { get; } = new(
        word => Commands.ContainsKey((NativeEnemyProjectileInstruction)word) ? 0 : null,
        (word, operands, cue, at) => AssertEqual(Commands[(NativeEnemyProjectileInstruction)word], cue.Kind, $"{at}: cue"),
        cue => cue.Kind == KagoBugProjectileCueKind.Delete);
}
