using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Kraid.KraidPartCue;
using P = SuperMetroid.Core.Bosses.Kraid.KraidPartSegment;
using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's typed part animations (#641, #1600, #1885): each foot, arm, lint and fingernail segment's beats, each beat naming
/// the installed frame it shows, and the animation starting at each.
/// </summary>
internal static class KraidAnimations
{
    /// <summary>Every foot, arm, lint and fingernail segment's beats.</summary>
    internal static AnimationSegments<KraidPartSegment, EnemyFrameId, KraidPartCue> Segments { get; } = new(
        new Dictionary<KraidPartSegment, AnimationBeat<EnemyFrameId, KraidPartCue>[]>
    {
        [P.FootInitial] =
        [
            Hold(F.KraidFootStanding),
        ],
        [P.FootNeutral] =
        [
            Hold(F.KraidFootNeutral),
        ],
        [P.FootWalkingForward] =
        [
            Show(F.KraidFootNeutral, 4),
            Show(F.KraidFootWalkingForwardSecond, 4),
            Show(F.KraidFootWalkingForwardThird, 4),
            Show(F.KraidFootWalkingForwardFourth, 4),
            Show(F.KraidFootWalkingForwardFifth, 4),
            Show(F.KraidFootWalkingForwardSixth, 4),
            Show(F.KraidFootWalkingForwardSeventh, 4),
            Show(F.KraidFootWalkingForwardEighth, 4),
            Show(F.KraidFootWalkingForwardNinth, 4),
            Show(F.KraidFootWalkingForwardTenth, 4),
            Show(F.KraidFootWalkingForwardEleventh, 4),
            Show(F.KraidFootWalkingForwardTwelfth, 3),
            Show(F.KraidFootWalkingForwardThirteenth, 1),
            Show(F.KraidFootWalkingForwardFourteenth, 1),
            Show(F.KraidFootWalkingForwardFifteenth, 1),
            Show(F.KraidFootWalkingForwardSixteenth, 1),
            Show(F.KraidFootWalkingForwardSeventeenth, 1),
            Show(F.KraidFootWalkingForwardEighteenth, 16),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardNineteenth, 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentieth, 1),
            Show(F.KraidFootWalkingForwardTwentieth, 3),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentieth, 1),
            Show(F.KraidFootWalkingForwardTwentieth, 3),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentyFirst, 1),
            Show(F.KraidFootWalkingForwardTwentyFirst, 3),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentySecond, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentyThird, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentyFourth, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Cue(K.StepSound),
            Show(F.KraidFootWalkingForwardTwentyFifth, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentySixth, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentySeventh, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentyEighth, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentyNinth, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardThirtieth, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardThirtyFirst, 1),
            Show(F.KraidFootNeutral, 1),
        ],
        [P.FootLungeForward] =
        [
            Show(F.KraidFootNeutral, 1),
            Show(F.KraidFootWalkingForwardSecond, 1),
            Show(F.KraidFootWalkingForwardThird, 1),
            Show(F.KraidFootWalkingForwardFourth, 1),
            Show(F.KraidFootWalkingForwardFifth, 1),
            Show(F.KraidFootWalkingForwardSixth, 1),
            Show(F.KraidFootWalkingForwardSeventh, 1),
            Show(F.KraidFootWalkingForwardEighth, 1),
            Show(F.KraidFootWalkingForwardNinth, 1),
            Show(F.KraidFootWalkingForwardTenth, 1),
            Show(F.KraidFootWalkingForwardEleventh, 1),
            Show(F.KraidFootWalkingForwardTwelfth, 1),
            Show(F.KraidFootWalkingForwardThirteenth, 1),
            Show(F.KraidFootWalkingForwardFourteenth, 1),
            Show(F.KraidFootWalkingForwardFifteenth, 1),
            Show(F.KraidFootWalkingForwardSixteenth, 1),
            Show(F.KraidFootWalkingForwardSeventeenth, 1),
            Show(F.KraidFootWalkingForwardEighteenth, 4),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardNineteenth, 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentieth, 1),
            Show(F.KraidFootWalkingForwardTwentieth, 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentieth, 1),
            Show(F.KraidFootWalkingForwardTwentieth, 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentyFirst, 1),
            Show(F.KraidFootWalkingForwardTwentyFirst, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentySecond, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentyThird, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentyFourth, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Cue(K.StepSound),
            Show(F.KraidFootWalkingForwardTwentyFifth, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentySixth, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentySeventh, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentyEighth, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardTwentyNinth, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootWalkingForwardThirtieth, 1),
            Show(F.KraidFootWalkingForwardThirtyFirst, 1),
            Cue(K.StepLeft),
            Show(F.KraidFootNeutral, 1),
        ],
        [P.FootWalkingBackward] =
        [
            Cue(K.StepRight),
            Show(F.KraidFootWalkingBackwardFirst, 4),
            Cue(K.StepRight),
            Show(F.KraidFootWalkingBackwardSecond, 1),
            Cue(K.StepRight),
            Show(F.KraidFootWalkingForwardThirtieth, 1),
            Cue(K.StepRight),
            Show(F.KraidFootWalkingForwardTwentyNinth, 1),
            Cue(K.StepRight),
            Show(F.KraidFootWalkingForwardTwentyEighth, 1),
            Cue(K.StepRight),
            Show(F.KraidFootWalkingForwardTwentySeventh, 1),
            Cue(K.StepRight),
            Show(F.KraidFootWalkingForwardTwentySixth, 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(F.KraidFootWalkingForwardTwentyFifth, 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(F.KraidFootWalkingForwardTwentyFourth, 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(F.KraidFootWalkingForwardTwentyThird, 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(F.KraidFootWalkingForwardTwentySecond, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Show(F.KraidFootWalkingForwardTwentyFirst, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Show(F.KraidFootWalkingBackwardThirteenth, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Show(F.KraidFootWalkingForwardTwentieth, 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepSound),
            Show(F.KraidFootWalkingForwardNineteenth, 1),
            Show(F.KraidFootWalkingForwardEighteenth, 20),
            Show(F.KraidFootWalkingForwardSeventeenth, 4),
            Show(F.KraidFootWalkingForwardSixteenth, 4),
            Show(F.KraidFootWalkingForwardFifteenth, 4),
            Show(F.KraidFootWalkingForwardFourteenth, 4),
            Show(F.KraidFootWalkingForwardThirteenth, 4),
            Show(F.KraidFootWalkingForwardTwelfth, 4),
            Show(F.KraidFootWalkingForwardEleventh, 4),
            Show(F.KraidFootWalkingForwardTenth, 4),
            Show(F.KraidFootWalkingForwardNinth, 1),
            Show(F.KraidFootWalkingForwardEighth, 1),
            Show(F.KraidFootWalkingForwardSeventh, 1),
            Show(F.KraidFootWalkingForwardSixth, 1),
            Show(F.KraidFootWalkingForwardFifth, 1),
            Show(F.KraidFootWalkingForwardFourth, 1),
            Show(F.KraidFootWalkingForwardThird, 1),
            Show(F.KraidFootWalkingForwardSecond, 1),
        ],
        [P.ArmNormal] =
        [
            Show(F.KraidArmNormalFirst, 6),
            Show(F.KraidArmNormalSecond, 6),
            Show(F.KraidArmNormalThird, 6),
            Show(F.KraidArmNormalFourth, 6),
            Show(F.KraidArmNormalFifth, 6),
            Show(F.KraidArmNormalSixth, 6),
            Show(F.KraidArmNormalSeventh, 6),
            Show(F.KraidArmNormalEighth, 6),
            Show(F.KraidArmNormalNinth, 6),
            Show(F.KraidArmNormalTenth, 6),
            Show(F.KraidArmNormalEighth, 6),
            Show(F.KraidArmNormalSeventh, 6),
            Show(F.KraidArmNormalSixth, 6),
            Show(F.KraidArmNormalFifth, 6),
            Show(F.KraidArmNormalFourth, 6),
            Show(F.KraidArmNormalThird, 6),
            Show(F.KraidArmNormalSecond, 6),
        ],
        [P.ArmNormalPause] =
        [
            Show(F.KraidArmNormalFirst, 32),
            Cue(K.SlowArmBelowHalfHealth),
        ],
        [P.ArmSlow] =
        [
            Show(F.KraidArmNormalFirst, 8),
            Show(F.KraidArmNormalSecond, 8),
            Show(F.KraidArmNormalThird, 8),
            Show(F.KraidArmNormalFourth, 8),
            Show(F.KraidArmNormalFifth, 8),
            Show(F.KraidArmNormalSixth, 8),
            Show(F.KraidArmNormalSeventh, 8),
            Show(F.KraidArmNormalEighth, 8),
            Show(F.KraidArmNormalNinth, 8),
            Show(F.KraidArmNormalTenth, 8),
            Show(F.KraidArmNormalEighth, 8),
            Show(F.KraidArmNormalSeventh, 8),
            Show(F.KraidArmNormalSixth, 8),
            Show(F.KraidArmNormalFifth, 8),
            Show(F.KraidArmNormalFourth, 8),
            Show(F.KraidArmNormalThird, 8),
            Show(F.KraidArmNormalSecond, 8),
            Show(F.KraidArmNormalFirst, 48),
            Cue(K.SlowArmBelowHalfHealth),
        ],
        [P.ArmRisingOrSinking] =
        [
            Show(F.KraidArmRisingOrSinkingFirst, 6),
            Show(F.KraidArmRisingOrSinkingSecond, 6),
            Show(F.KraidArmRisingOrSinkingThird, 6),
            Show(F.KraidArmRisingOrSinkingFourth, 6),
            Show(F.KraidArmRisingOrSinkingFifth, 6),
            Show(F.KraidArmRisingOrSinkingSixth, 6),
            Show(F.KraidArmRisingOrSinkingSeventh, 6),
            Show(F.KraidArmRisingOrSinkingEighth, 6),
            Show(F.KraidArmRisingOrSinkingNinth, 6),
            Show(F.KraidArmRisingOrSinkingTenth, 6),
            Show(F.KraidArmRisingOrSinkingEighth, 6),
            Show(F.KraidArmRisingOrSinkingSeventh, 6),
            Show(F.KraidArmRisingOrSinkingSixth, 6),
            Show(F.KraidArmRisingOrSinkingFifth, 6),
            Show(F.KraidArmRisingOrSinkingFourth, 6),
            Show(F.KraidArmRisingOrSinkingThird, 6),
            Show(F.KraidArmRisingOrSinkingSecond, 6),
            Show(F.KraidArmRisingOrSinkingFirst, 32),
        ],
        [P.ArmDyingOrPreparingToLunge] =
        [
            Show(F.KraidArmNormalFirst, 6),
            Show(F.KraidArmDyingOrPreparingToLungeSecond, 6),
            Show(F.KraidArmDyingOrPreparingToLungeThird, ReadyToLungeArmDuration),
        ],
        [P.LintInitial] =
        [
            Hold(F.KraidLintInitial),
        ],
        [P.LintPostGrowth] =
        [
            Hold(F.KraidLintPostGrowth),
        ],
        [P.NailLoop] =
        [
            Show(F.KraidNailLoopFirst, 3),
            Show(F.KraidNailLoopSecond, 3),
            Show(F.KraidNailLoopThird, 3),
            Show(F.KraidNailLoopFourth, 3),
            Show(F.KraidNailLoopFifth, 3),
            Show(F.KraidNailLoopSixth, 3),
            Show(F.KraidNailLoopSeventh, 3),
            Show(F.KraidNailLoopEighth, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KraidPartSegment, Animation<KraidPartSegment>> StartingAt { get; } = new Dictionary<KraidPartSegment, Animation<KraidPartSegment>>
    {
        [P.FootInitial] = new([new(P.FootInitial, 0, 1, 1)], AnimationEnd.Holds),
        [P.FootNeutral] = new([new(P.FootNeutral, 0, 1, 1)], AnimationEnd.Holds),
        [P.FootWalkingForward] = new([new(P.FootWalkingForward, 0, 59, 1)], AnimationEnd.Holds),
        [P.FootLungeForward] = new([new(P.FootLungeForward, 0, 59, 1)], AnimationEnd.Holds),
        [P.FootWalkingBackward] = new([new(P.FootWalkingBackward, 0, 55, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmNormal] = new([new(P.ArmNormal, 0, 17, 1), new(P.ArmNormalPause, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmNormalPause] = new([new(P.ArmNormalPause, 0, 2, 1), new(P.ArmNormal, 0, 17, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmSlow] = new([new(P.ArmSlow, 0, 19, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmRisingOrSinking] = new([new(P.ArmRisingOrSinking, 0, 18, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmDyingOrPreparingToLunge] = new([new(P.ArmDyingOrPreparingToLunge, 0, 3, 1)], AnimationEnd.Holds),
        [P.LintInitial] = new([new(P.LintInitial, 0, 1, 1)], AnimationEnd.Holds),
        [P.LintPostGrowth] = new([new(P.LintPostGrowth, 0, 1, 1)], AnimationEnd.Holds),
        [P.NailLoop] = new([new(P.NailLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KraidPartSegment> StartingWith(KraidPartSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, KraidPartCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, KraidPartCue>.Frame(frame, duration);

    /// <summary>
    /// Shows <paramref name="frame"/> for good (#1627): the part rests on it until its owner starts another animation. The
    /// feet's standing poses and the belly lints' poses are held: nothing reads a foot's frame timer, and a lint's behaviour
    /// reloads its timer every update, so the longest frame duration never runs out where gameplay sees it.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, KraidPartCue> Hold(EnemyFrameId frame) =>
        new AnimationBeat<EnemyFrameId, KraidPartCue>.HeldFrame(frame);

    /// <summary>
    /// The updates the arm's last lunge-ready pose shows, the longest duration a frame can have, kept exact (#1627): while mouth
    /// reopens are pending, Kraid's arm behaviour adds an update to the arm's frame timer every update, which keeps this
    /// count standing and the arm's new-frame flag clear; a held frame's timer would run out every update instead.
    /// </summary>
    private const int ReadyToLungeArmDuration = 32767;

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, KraidPartCue> Cue(KraidPartCue cue) =>
        new AnimationBeat<EnemyFrameId, KraidPartCue>.Event(cue);
}
