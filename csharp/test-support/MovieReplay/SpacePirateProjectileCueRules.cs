using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>How the laser and claw typed programs match their native lists: each command and the cue it becomes.</summary>
internal static class SpacePirateProjectileCueRules
{
    /// <summary>Each command the lists use and the cue it becomes; both carry the movement routine as a word operand.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, SpacePirateProjectileCueKind> Commands = new()
    {
        [NativeEnemyProjectileInstruction.FrameBehaviorInY] = SpacePirateProjectileCueKind.ThrowClaw,
        [NativeEnemyProjectileInstruction.FrameBehaviorInY_ExecuteY] = SpacePirateProjectileCueKind.FireLaser,
    };

    /// <summary>The oracle rules for the laser and claw programs.</summary>
    internal static NativeEnemyProjectileCueRules<SpacePirateProjectileCue> Rules { get; } = new(
        word => Commands.ContainsKey((NativeEnemyProjectileInstruction)word) ? 2 : null,
        (word, operands, cue, at) =>
        {
            VerificationAssertions.AssertEqual(Commands[(NativeEnemyProjectileInstruction)word], cue.Kind, $"{at}: cue");
            // Both ways' routines stand for the one motion; the routine the list installs names the cue's way (#2278).
            ushort routine = (ushort)(operands[0] | operands[1] << 8);
            VerificationAssertions.AssertTrue(EnemyProjectileMotionAccess.StandsFor(routine, SingleMotion.Moving), $"{at}: movement");
            VerificationAssertions.AssertEqual(cue.Kind == SpacePirateProjectileCueKind.FireLaser
                ? EnemyProjectileMotionAccess.PirateLaserFlight(cue.Direction)
                : EnemyProjectileMotionAccess.PirateClawThrow(cue.Direction), routine, $"{at}: the movement's way");
        });
}
