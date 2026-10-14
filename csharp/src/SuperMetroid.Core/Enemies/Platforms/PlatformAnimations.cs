using K = SuperMetroid.Core.Enemies.Platforms.PlatformCueKind;
using P = SuperMetroid.Core.Enemies.Platforms.PlatformProgram;
using SuperMetroid.Core.Enemies.Platforms.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The Tripper's and Kamer's vertically moving and still platform loops as typed programs (#641), in place of their bank-$A3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="PlatformFrame"/> pose and each callback becomes a <see cref="PlatformCue"/> with its typed operands.
/// </summary>
internal static class PlatformAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<PlatformProgram, PlatformFrame, PlatformCue> Programs { get; } = new(
        new Dictionary<PlatformProgram, AnimationProgramStep<PlatformProgram, PlatformFrame, PlatformCue>[]>
    {
        // $A3:9BBB
        [P.KamerMovingLeft] =
        [
            Cue(new(K.MoveLeft)),
            FallInto(P.KamerMovingLeftLoop),
        ],
        // $A3:9BBD
        [P.KamerMovingLeftLoop] =
        [
            Show(12, 10),
            Show(13, 10),
            Show(14, 10),
            Show(15, 10),
            GoTo(P.KamerMovingLeftLoop),
        ],
        // $A3:9BD1
        [P.KamerMovingRight] =
        [
            Cue(new(K.MoveRight)),
            FallInto(P.KamerMovingRightLoop),
        ],
        // $A3:9BD3
        [P.KamerMovingRightLoop] =
        [
            Show(12, 10),
            Show(13, 10),
            Show(14, 10),
            Show(15, 10),
            GoTo(P.KamerMovingRightLoop),
        ],
        // $A3:9BE7
        [P.KamerStillLeft] =
        [
            Cue(new(K.MoveLeft)),
            FallInto(P.KamerStillLeftLoop),
        ],
        // $A3:9BE9
        [P.KamerStillLeftLoop] =
        [
            Show(12, 10),
            Show(13, 10),
            Show(14, 10),
            Show(15, 10),
            GoTo(P.KamerStillLeftLoop),
        ],
        // $A3:9BFD
        [P.KamerStillRight] =
        [
            Cue(new(K.MoveRight)),
            FallInto(P.KamerStillRightLoop),
        ],
        // $A3:9BFF
        [P.KamerStillRightLoop] =
        [
            Show(12, 10),
            Show(13, 10),
            Show(14, 10),
            Show(15, 10),
            GoTo(P.KamerStillRightLoop),
        ],
        // $A3:9C13
        [P.TripperMovingLeft] =
        [
            Cue(new(K.MoveLeft)),
            FallInto(P.TripperMovingLeftLoop),
        ],
        // $A3:9C15
        [P.TripperMovingLeftLoop] =
        [
            Show(0, 7),
            Show(1, 8),
            Show(0, 7),
            Show(2, 8),
            GoTo(P.TripperMovingLeftLoop),
        ],
        // $A3:9C29
        [P.TripperMovingRight] =
        [
            Cue(new(K.MoveRight)),
            FallInto(P.TripperMovingRightLoop),
        ],
        // $A3:9C2B
        [P.TripperMovingRightLoop] =
        [
            Show(3, 7),
            Show(4, 8),
            Show(3, 7),
            Show(5, 8),
            GoTo(P.TripperMovingRightLoop),
        ],
        // $A3:9C3F
        [P.TripperStillMovingLeft] =
        [
            Cue(new(K.MoveLeft)),
            FallInto(P.TripperStillMovingLeftLoop),
        ],
        // $A3:9C41
        [P.TripperStillMovingLeftLoop] =
        [
            Show(6, 7),
            Show(7, 8),
            Show(6, 7),
            Show(8, 8),
            GoTo(P.TripperStillMovingLeftLoop),
        ],
        // $A3:9C55
        [P.TripperStillMovingRight] =
        [
            Cue(new(K.MoveRight)),
            FallInto(P.TripperStillMovingRightLoop),
        ],
        // $A3:9C57
        [P.TripperStillMovingRightLoop] =
        [
            Show(9, 7),
            Show(10, 8),
            Show(9, 7),
            Show(11, 8),
            GoTo(P.TripperStillMovingRightLoop),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params PlatformProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => PlatformFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<PlatformProgram, PlatformFrame, PlatformCue> Show(int pose, int duration) => AnimationProgramStep<PlatformProgram, PlatformFrame, PlatformCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<PlatformProgram, PlatformFrame, PlatformCue> Cue(PlatformCue cue) => AnimationProgramStep<PlatformProgram, PlatformFrame, PlatformCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<PlatformProgram, PlatformFrame, PlatformCue> GoTo(PlatformProgram program) => AnimationProgramStep<PlatformProgram, PlatformFrame, PlatformCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<PlatformProgram, PlatformFrame, PlatformCue> FallInto(PlatformProgram program) => AnimationProgramStep<PlatformProgram, PlatformFrame, PlatformCue>.FallInto(program);

}
