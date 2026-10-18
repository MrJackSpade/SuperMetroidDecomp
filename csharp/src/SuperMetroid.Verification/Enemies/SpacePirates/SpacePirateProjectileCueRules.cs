using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>How the laser and claw typed programs match their native lists: each command and the cue it becomes.</summary>
internal static class SpacePirateProjectileCueRules
{
    /// <summary>Each command the lists use and the cue it becomes; both carry the movement routine as a word operand.</summary>
    private static readonly Dictionary<EnemyProjectileInstruction, SpacePirateProjectileCueKind> Commands = new()
    {
        [EnemyProjectileInstruction.PreInstructionInY] = SpacePirateProjectileCueKind.StartMoving,
        [EnemyProjectileInstruction.PreInstructionInY_ExecuteY] = SpacePirateProjectileCueKind.StartMovingNow,
    };

    /// <summary>The oracle rules for the laser and claw programs.</summary>
    internal static NativeEnemyProjectileCueRules<SpacePirateProjectileCue> Rules { get; } = new(
        word => Commands.ContainsKey((EnemyProjectileInstruction)word) ? 2 : null,
        (word, operands, cue, at) =>
        {
            AssertEqual(Commands[(EnemyProjectileInstruction)word], cue.Kind, $"{at}: cue");
            AssertEqual((ushort)(operands[0] | operands[1] << 8), EnemyProjectilePreInstructionAccess.Word(cue.Movement), $"{at}: movement");
        });
}
