using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>How the thrown lava's typed programs match its native lists: each command and the cue it becomes.</summary>
internal static class MagdolliteLavaCueRules
{
    /// <summary>Each command the lava's lists use and the cue it becomes; neither carries an operand.</summary>
    private static readonly Dictionary<EnemyProjectileInstruction, MagdolliteLavaCueKind> Commands = new()
    {
        [EnemyProjectileInstruction.Delete] = MagdolliteLavaCueKind.Delete,
        [EnemyProjectileInstruction.MagdolliteFlame_SpawnDrops] = MagdolliteLavaCueKind.SpawnDrops,
    };

    /// <summary>The oracle rules for the lava's programs.</summary>
    internal static NativeEnemyProjectileCueRules<MagdolliteLavaCue> Rules { get; } = new(
        word => Commands.ContainsKey((EnemyProjectileInstruction)word) ? 0 : null,
        (word, operands, cue, at) => AssertEqual(Commands[(EnemyProjectileInstruction)word], cue.Kind, $"{at}: cue"),
        cue => cue.Kind == MagdolliteLavaCueKind.Delete);
}
