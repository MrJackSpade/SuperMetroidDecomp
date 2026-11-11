using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Platforms.PlatformCueKind;
using P = SuperMetroid.Core.Enemies.Platforms.PlatformProgram;

namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The Tripper's and Kamer's vertically moving and still platform loops as typed programs (#641). Each
/// frame shows a <see cref="PlatformFrame"/> pose and each callback becomes a <see cref="PlatformCue"/> with its typed operands.
/// </summary>
internal static class PlatformAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PlatformProgram, PlatformFrame, PlatformCue> Segments { get; } = new(
        new Dictionary<PlatformProgram, AnimationBeat<PlatformFrame, PlatformCue>[]>
    {
        [P.KamerMovingLeft] =
        [
            Cue(new(K.MoveLeft)),
        ],
        [P.KamerMovingLeftLoop] =
        [
            Show(12, 10),
            Show(13, 10),
            Show(14, 10),
            Show(15, 10),
        ],
        [P.KamerMovingRight] =
        [
            Cue(new(K.MoveRight)),
        ],
        [P.KamerMovingRightLoop] =
        [
            Show(12, 10),
            Show(13, 10),
            Show(14, 10),
            Show(15, 10),
        ],
        [P.KamerStillLeft] =
        [
            Cue(new(K.MoveLeft)),
        ],
        [P.KamerStillLeftLoop] =
        [
            Show(12, 10),
            Show(13, 10),
            Show(14, 10),
            Show(15, 10),
        ],
        [P.KamerStillRight] =
        [
            Cue(new(K.MoveRight)),
        ],
        [P.KamerStillRightLoop] =
        [
            Show(12, 10),
            Show(13, 10),
            Show(14, 10),
            Show(15, 10),
        ],
        [P.TripperMovingLeft] =
        [
            Cue(new(K.MoveLeft)),
        ],
        [P.TripperMovingLeftLoop] =
        [
            Show(0, 7),
            Show(1, 8),
            Show(0, 7),
            Show(2, 8),
        ],
        [P.TripperMovingRight] =
        [
            Cue(new(K.MoveRight)),
        ],
        [P.TripperMovingRightLoop] =
        [
            Show(3, 7),
            Show(4, 8),
            Show(3, 7),
            Show(5, 8),
        ],
        [P.TripperStillMovingLeft] =
        [
            Cue(new(K.MoveLeft)),
        ],
        [P.TripperStillMovingLeftLoop] =
        [
            Show(6, 7),
            Show(7, 8),
            Show(6, 7),
            Show(8, 8),
        ],
        [P.TripperStillMovingRight] =
        [
            Cue(new(K.MoveRight)),
        ],
        [P.TripperStillMovingRightLoop] =
        [
            Show(9, 7),
            Show(10, 8),
            Show(9, 7),
            Show(11, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PlatformProgram, Animation<PlatformProgram>> StartingAt { get; } = new Dictionary<PlatformProgram, Animation<PlatformProgram>>
    {
        [P.KamerMovingLeft] = new([new(P.KamerMovingLeft, 0, 1, 1), new(P.KamerMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.KamerMovingLeftLoop] = new([new(P.KamerMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.KamerMovingRight] = new([new(P.KamerMovingRight, 0, 1, 1), new(P.KamerMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.KamerMovingRightLoop] = new([new(P.KamerMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.KamerStillLeft] = new([new(P.KamerStillLeft, 0, 1, 1), new(P.KamerStillLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.KamerStillLeftLoop] = new([new(P.KamerStillLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.KamerStillRight] = new([new(P.KamerStillRight, 0, 1, 1), new(P.KamerStillRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.KamerStillRightLoop] = new([new(P.KamerStillRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TripperMovingLeft] = new([new(P.TripperMovingLeft, 0, 1, 1), new(P.TripperMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TripperMovingLeftLoop] = new([new(P.TripperMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TripperMovingRight] = new([new(P.TripperMovingRight, 0, 1, 1), new(P.TripperMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TripperMovingRightLoop] = new([new(P.TripperMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TripperStillMovingLeft] = new([new(P.TripperStillMovingLeft, 0, 1, 1), new(P.TripperStillMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TripperStillMovingLeftLoop] = new([new(P.TripperStillMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TripperStillMovingRight] = new([new(P.TripperStillMovingRight, 0, 1, 1), new(P.TripperStillMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TripperStillMovingRightLoop] = new([new(P.TripperStillMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PlatformProgram> StartingWith(PlatformProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<PlatformFrame, PlatformCue> Show(int pose, int duration) => new AnimationBeat<PlatformFrame, PlatformCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<PlatformFrame, PlatformCue> Cue(PlatformCue cue) => new AnimationBeat<PlatformFrame, PlatformCue>.Event(cue);
}
