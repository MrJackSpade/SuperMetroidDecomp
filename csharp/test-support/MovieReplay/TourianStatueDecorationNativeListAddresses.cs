using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists of the Tourian statue scene's base decoration, Ridley and Phantoon (#2554), which the statue
/// spawns as decorations of their own: each program's entry point, the one command their lists use, and the family that
/// binds the typed programs to them. Only verification reads them; the port runs typed programs.
/// </summary>
internal static class TourianStatueDecorationNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<TourianStatueDecorationSegment>[] Entries =
    [
        new NativeProgramEntry<TourianStatueDecorationSegment>(TourianStatueDecorationSegment.BaseDecoration, TourianStatueProjectileInstructionProgramDefinitionsConstants.BaseDecoration),
        new NativeProgramEntry<TourianStatueDecorationSegment>(TourianStatueDecorationSegment.Ridley, TourianStatueProjectileInstructionProgramDefinitionsConstants.Ridley),
        new NativeProgramEntry<TourianStatueDecorationSegment>(TourianStatueDecorationSegment.Phantoon, TourianStatueProjectileInstructionProgramDefinitionsConstants.Phantoon),
    ];

    /// <summary>The routine-setting command, which takes the routine's two operand bytes.</summary>
    private const ushort SetsRoutine = (ushort)NativeEnemyProjectileInstruction.FrameBehaviorInY;

    /// <summary>
    /// The oracle rules for the decorations' programs: setting the reveal-following routine is the cue that starts the base
    /// decoration following the reveal.
    /// </summary>
    private static readonly NativeEnemyProjectileCueRules<TourianStatueDecorationCue> Rules = new(
        word => word == SetsRoutine ? 2 : null,
        (word, operands, cue, at) =>
        {
            VerificationAssertions.AssertEqual(SetsRoutine, word, $"{at}: the decorations' lists set only their routine");
            VerificationAssertions.AssertEqual(TourianStatueDecorationCue.FollowReveal, cue, $"{at}: cue");
            VerificationAssertions.AssertEqual<Enum?>(TourianStatueDecorationMotion.FollowingReveal,
                EnemyProjectileMotionAccess.FromWord((ushort)(operands[0] | operands[1] << 8)), $"{at}: movement");
        });

    /// <summary>The family's typed animations beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, TourianStatueDecorationAnimations.Segments,
        TourianStatueDecorationAnimations.StartingAt,
        EnemyProjectileProgramFrameOperands.KeyOf, Rules);
}
