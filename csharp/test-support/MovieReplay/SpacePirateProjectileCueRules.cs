using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>How the laser and claw typed programs match their native lists: the command each fires or throws by.</summary>
internal static class SpacePirateProjectileCueRules
{
    /// <summary>The command a laser list fires its laser by, carrying the movement routine as a word operand.</summary>
    private const NativeEnemyProjectileInstruction LaserCommand = NativeEnemyProjectileInstruction.FrameBehaviorInY_ExecuteY;

    /// <summary>The command a claw list throws its claw by, carrying the movement routine as a word operand.</summary>
    private const NativeEnemyProjectileInstruction ClawCommand = NativeEnemyProjectileInstruction.FrameBehaviorInY;

    /// <summary>The oracle rules for the laser programs.</summary>
    internal static NativeEnemyProjectileCueRules<SpacePirateLaserCue> LaserRules { get; } = new(
        word => (NativeEnemyProjectileInstruction)word == LaserCommand ? 2 : null,
        (word, operands, cue, at) =>
        {
            VerificationAssertions.AssertEqual(LaserCommand, (NativeEnemyProjectileInstruction)word, $"{at}: cue");
            ushort routine = CheckedRoutine(operands, at);
            VerificationAssertions.AssertEqual(EnemyProjectileMotionAccess.PirateLaserFlight(cue.Direction), routine, $"{at}: the movement's way");
        });

    /// <summary>The oracle rules for the claw programs.</summary>
    internal static NativeEnemyProjectileCueRules<SpacePirateClawCue> ClawRules { get; } = new(
        word => (NativeEnemyProjectileInstruction)word == ClawCommand ? 2 : null,
        (word, operands, cue, at) =>
        {
            VerificationAssertions.AssertEqual(ClawCommand, (NativeEnemyProjectileInstruction)word, $"{at}: cue");
            ushort routine = CheckedRoutine(operands, at);
            VerificationAssertions.AssertEqual(EnemyProjectileMotionAccess.PirateClawThrow(cue.Direction), routine, $"{at}: the movement's way");
        });

    /// <summary>
    /// The movement routine a command installs, checked to stand for the one motion: both ways' routines stand for it, and
    /// the routine names the cue's way (#2278).
    /// </summary>
    private static ushort CheckedRoutine(byte[] operands, string at)
    {
        ushort routine = (ushort)(operands[0] | operands[1] << 8);
        VerificationAssertions.AssertEqual<Enum?>(SingleMotion.Moving, EnemyProjectileMotionAccess.FromWord(routine), $"{at}: movement");
        return routine;
    }
}
