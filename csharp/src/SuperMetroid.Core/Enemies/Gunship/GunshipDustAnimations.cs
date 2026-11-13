using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Gunship.GunshipDustSegment;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship liftoff dust clouds' typed animations (#641). Each cloud loads its general
/// timer with one, shows its poses, counts the timer down past its single repeat and deletes itself.
/// </summary>
internal static class GunshipDustAnimations
{
    /// <summary>Every dust animation.</summary>
    internal static AnimationSegments<GunshipDustSegment, GunshipDustFrame, GunshipDustCue> Segments { get; } = new(
        new Dictionary<GunshipDustSegment, AnimationBeat<GunshipDustFrame, GunshipDustCue>[]>
    {
        [P.RightInner] =
        [
        ],
        [P.RightInnerLoop] =
        [
            Show(P.RightInnerLoop, 0, 8),
            Show(P.RightInnerLoop, 1, 8),
            Show(P.RightInnerLoop, 2, 9),
            Show(P.RightInnerLoop, 3, 9),
            Show(P.RightInnerLoop, 4, 10),
            Show(P.RightInnerLoop, 5, 10),
            Show(P.RightInnerLoop, 6, 10),
            Show(P.RightInnerLoop, 7, 10),
            Cue(GunshipDustCue.Delete),
        ],
        [P.RightMiddle] =
        [
        ],
        [P.RightMiddleLoop] =
        [
            Show(P.RightMiddleLoop, 0, 6),
            Show(P.RightMiddleLoop, 1, 6),
            Show(P.RightMiddleLoop, 2, 7),
            Show(P.RightMiddleLoop, 3, 7),
            Show(P.RightMiddleLoop, 4, 8),
            Show(P.RightMiddleLoop, 5, 8),
            Show(P.RightMiddleLoop, 6, 8),
            Show(P.RightMiddleLoop, 7, 8),
            Cue(GunshipDustCue.Delete),
        ],
        [P.RightOuter] =
        [
        ],
        [P.RightOuterLoop] =
        [
            Show(P.RightOuterLoop, 0, 11),
            Show(P.RightOuterLoop, 1, 11),
            Show(P.RightOuterLoop, 2, 12),
            Show(P.RightOuterLoop, 3, 12),
            Show(P.RightOuterLoop, 4, 13),
            Show(P.RightOuterLoop, 5, 13),
            Show(P.RightOuterLoop, 6, 13),
            Cue(GunshipDustCue.Delete),
        ],
        [P.LeftInner] =
        [
        ],
        [P.LeftInnerLoop] =
        [
            Show(P.LeftInnerLoop, 0, 8),
            Show(P.LeftInnerLoop, 1, 8),
            Show(P.LeftInnerLoop, 2, 9),
            Show(P.LeftInnerLoop, 3, 9),
            Show(P.LeftInnerLoop, 4, 10),
            Show(P.LeftInnerLoop, 5, 10),
            Show(P.LeftInnerLoop, 6, 10),
            Show(P.LeftInnerLoop, 7, 10),
            Cue(GunshipDustCue.Delete),
        ],
        [P.LeftMiddle] =
        [
        ],
        [P.LeftMiddleLoop] =
        [
            Show(P.LeftMiddleLoop, 0, 6),
            Show(P.LeftMiddleLoop, 1, 6),
            Show(P.LeftMiddleLoop, 2, 7),
            Show(P.LeftMiddleLoop, 3, 7),
            Show(P.LeftMiddleLoop, 4, 8),
            Show(P.LeftMiddleLoop, 5, 8),
            Show(P.LeftMiddleLoop, 6, 8),
            Show(P.LeftMiddleLoop, 7, 8),
            Cue(GunshipDustCue.Delete),
        ],
        [P.LeftOuter] =
        [
        ],
        [P.LeftOuterLoop] =
        [
            Show(P.LeftOuterLoop, 0, 11),
            Show(P.LeftOuterLoop, 1, 11),
            Show(P.LeftOuterLoop, 2, 12),
            Show(P.LeftOuterLoop, 3, 12),
            Show(P.LeftOuterLoop, 4, 13),
            Show(P.LeftOuterLoop, 5, 13),
            Show(P.LeftOuterLoop, 6, 13),
            Cue(GunshipDustCue.Delete),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<GunshipDustSegment, Animation<GunshipDustSegment>> StartingAt { get; } = new Dictionary<GunshipDustSegment, Animation<GunshipDustSegment>>
    {
        [P.RightInner] = new([new(P.RightInnerLoop, 0, 8, 1, AnimationPhaseCounting.EveryPlay), new(P.RightInnerLoop, 8, 1, 1)], AnimationEnd.Holds),
        [P.RightMiddle] = new([new(P.RightMiddleLoop, 0, 8, 1, AnimationPhaseCounting.EveryPlay), new(P.RightMiddleLoop, 8, 1, 1)], AnimationEnd.Holds),
        [P.RightOuter] = new([new(P.RightOuterLoop, 0, 7, 1, AnimationPhaseCounting.EveryPlay), new(P.RightOuterLoop, 7, 1, 1)], AnimationEnd.Holds),
        [P.LeftInner] = new([new(P.LeftInnerLoop, 0, 8, 1, AnimationPhaseCounting.EveryPlay), new(P.LeftInnerLoop, 8, 1, 1)], AnimationEnd.Holds),
        [P.LeftMiddle] = new([new(P.LeftMiddleLoop, 0, 8, 1, AnimationPhaseCounting.EveryPlay), new(P.LeftMiddleLoop, 8, 1, 1)], AnimationEnd.Holds),
        [P.LeftOuter] = new([new(P.LeftOuterLoop, 0, 7, 1, AnimationPhaseCounting.EveryPlay), new(P.LeftOuterLoop, 7, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<GunshipDustSegment> StartingWith(GunshipDustSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="segment"/>'s pose <paramref name="phase"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<GunshipDustFrame, GunshipDustCue> Show(GunshipDustSegment segment, int phase, int duration) => new AnimationBeat<GunshipDustFrame, GunshipDustCue>.Frame(new(segment, phase), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<GunshipDustFrame, GunshipDustCue> Cue(GunshipDustCue cue) => new AnimationBeat<GunshipDustFrame, GunshipDustCue>.Event(cue);
}
