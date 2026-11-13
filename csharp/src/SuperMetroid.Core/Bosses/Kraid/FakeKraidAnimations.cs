using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Kraid.FakeKraidCueKind;
using P = SuperMetroid.Core.Bosses.Kraid.FakeKraidProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Mini-Kraid's action choice, step and spit animations as typed programs (#641), in place of their lists. Generated from the verified lists: each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="FakeKraidCue"/> with its typed operands.
/// </summary>
internal static class FakeKraidAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FakeKraidProgram, EnemyFrameId, FakeKraidCue> Segments { get; } = new(
        new Dictionary<FakeKraidProgram, AnimationBeat<EnemyFrameId, FakeKraidCue>[]>
    {
        [P.ChooseActionFacingLeft] =
        [
            Cue(new(K.ChooseAction)),
        ],
        [P.StepForwardsFacingLeft] =
        [
            Show(EnemyFrameId.FakeKraidWalkLeft0, 16),
            Show(EnemyFrameId.FakeKraidWalkLeft1, 12),
            Show(EnemyFrameId.FakeKraidWalkLeft2, 8),
            Show(EnemyFrameId.FakeKraidWalkLeft3, 12),
            Cue(new(K.Move)),
        ],
        [P.ChooseAlternateActionFacingLeft] =
        [
            Cue(new(K.ChooseAction)),
        ],
        [P.StepBackwardsFacingLeft] =
        [
            Show(EnemyFrameId.FakeKraidWalkLeft0, 16),
            Cue(new(K.Move)),
            Show(EnemyFrameId.FakeKraidWalkLeft3, 12),
            Show(EnemyFrameId.FakeKraidWalkLeft2, 8),
            Show(EnemyFrameId.FakeKraidWalkLeft1, 12),
        ],
        [P.FireSpitFacingLeft] =
        [
            Show(EnemyFrameId.FakeKraidSpitLeft0, 16),
            Cue(new(K.PlayCrySound)),
            Show(EnemyFrameId.FakeKraidSpitLeft1, 8),
            Cue(new(K.SpitLeft)),
            Show(EnemyFrameId.FakeKraidSpitLeft2, 16),
            Show(EnemyFrameId.FakeKraidSpitLeft1, 8),
        ],
        [P.ChooseActionFacingRight] =
        [
            Cue(new(K.ChooseAction)),
        ],
        [P.StepForwardsFacingRight] =
        [
            Show(EnemyFrameId.FakeKraidWalkRight0, 16),
            Show(EnemyFrameId.FakeKraidWalkRight1, 12),
            Show(EnemyFrameId.FakeKraidWalkRight2, 8),
            Show(EnemyFrameId.FakeKraidWalkRight3, 12),
            Cue(new(K.Move)),
        ],
        [P.ChooseAlternateActionFacingRight] =
        [
            Cue(new(K.ChooseAction)),
        ],
        [P.StepBackwardsFacingRight] =
        [
            Show(EnemyFrameId.FakeKraidWalkRight0, 16),
            Cue(new(K.Move)),
            Show(EnemyFrameId.FakeKraidWalkRight3, 12),
            Show(EnemyFrameId.FakeKraidWalkRight2, 8),
            Show(EnemyFrameId.FakeKraidWalkRight1, 12),
        ],
        [P.FireSpitFacingRight] =
        [
            Show(EnemyFrameId.FakeKraidSpitRight0, 16),
            Cue(new(K.PlayCrySound)),
            Show(EnemyFrameId.FakeKraidSpitRight1, 8),
            Cue(new(K.SpitRight)),
            Show(EnemyFrameId.FakeKraidSpitRight2, 16),
            Show(EnemyFrameId.FakeKraidSpitRight1, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FakeKraidProgram, Animation<FakeKraidProgram>> StartingAt { get; } = new Dictionary<FakeKraidProgram, Animation<FakeKraidProgram>>
    {
        [P.ChooseActionFacingLeft] = new([new(P.ChooseActionFacingLeft, 0, 1, 1), new(P.StepForwardsFacingLeft, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StepForwardsFacingLeft] = new([new(P.StepForwardsFacingLeft, 0, 5, 1), new(P.ChooseActionFacingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChooseAlternateActionFacingLeft] = new([new(P.ChooseAlternateActionFacingLeft, 0, 1, 1), new(P.StepBackwardsFacingLeft, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StepBackwardsFacingLeft] = new([new(P.StepBackwardsFacingLeft, 0, 5, 1), new(P.ChooseAlternateActionFacingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FireSpitFacingLeft] = new([new(P.FireSpitFacingLeft, 0, 6, 1), new(P.ChooseActionFacingLeft, 0, 1, 1), new(P.StepForwardsFacingLeft, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChooseActionFacingRight] = new([new(P.ChooseActionFacingRight, 0, 1, 1), new(P.StepForwardsFacingRight, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StepForwardsFacingRight] = new([new(P.StepForwardsFacingRight, 0, 5, 1), new(P.ChooseActionFacingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChooseAlternateActionFacingRight] = new([new(P.ChooseAlternateActionFacingRight, 0, 1, 1), new(P.StepBackwardsFacingRight, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StepBackwardsFacingRight] = new([new(P.StepBackwardsFacingRight, 0, 5, 1), new(P.ChooseAlternateActionFacingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FireSpitFacingRight] = new([new(P.FireSpitFacingRight, 0, 6, 1), new(P.ChooseActionFacingRight, 0, 1, 1), new(P.StepForwardsFacingRight, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FakeKraidProgram> StartingWith(FakeKraidProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, FakeKraidCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, FakeKraidCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, FakeKraidCue> Cue(FakeKraidCue cue) => new AnimationBeat<EnemyFrameId, FakeKraidCue>.Event(cue);
}
