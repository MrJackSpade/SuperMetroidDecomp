using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Gunship.GunshipDustProgram;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship liftoff dust clouds' typed programs (#641), in place of their bank-$86 lists. Each cloud loads its general
/// timer with one, shows its poses, counts the timer down past its single repeat and deletes itself.
/// </summary>
internal static class GunshipDustAnimations
{
    /// <summary>Every dust program.</summary>
    internal static AnimationProgramSet<GunshipDustProgram, GunshipDustFrame, GunshipDustCue> Programs { get; } = new(
        new Dictionary<GunshipDustProgram, AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue>[]>
    {
        [P.RightInner] =
        [
            SetCounter(1),
            FallInto(P.RightInnerLoop),
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
            RepeatWhileCounting(P.RightInnerLoop),
            Cue(GunshipDustCue.Delete),
            Hold,
        ],
        [P.RightMiddle] =
        [
            SetCounter(1),
            FallInto(P.RightMiddleLoop),
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
            RepeatWhileCounting(P.RightMiddleLoop),
            Cue(GunshipDustCue.Delete),
            Hold,
        ],
        [P.RightOuter] =
        [
            SetCounter(1),
            FallInto(P.RightOuterLoop),
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
            RepeatWhileCounting(P.RightOuterLoop),
            Cue(GunshipDustCue.Delete),
            Hold,
        ],
        [P.LeftInner] =
        [
            SetCounter(1),
            FallInto(P.LeftInnerLoop),
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
            RepeatWhileCounting(P.LeftInnerLoop),
            Cue(GunshipDustCue.Delete),
            Hold,
        ],
        [P.LeftMiddle] =
        [
            SetCounter(1),
            FallInto(P.LeftMiddleLoop),
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
            RepeatWhileCounting(P.LeftMiddleLoop),
            Cue(GunshipDustCue.Delete),
            Hold,
        ],
        [P.LeftOuter] =
        [
            SetCounter(1),
            FallInto(P.LeftOuterLoop),
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
            RepeatWhileCounting(P.LeftOuterLoop),
            Cue(GunshipDustCue.Delete),
            Hold,
        ],
    });

    /// <summary>Shows <paramref name="program"/>'s pose <paramref name="phase"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue> Show(GunshipDustProgram program, int phase, int duration) => AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue>.Show(new(program, phase), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue> Cue(GunshipDustCue cue) => AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue>.Publish(cue);

    /// <summary>Loads the general timer with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue> SetCounter(int count) => AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue>.SetCounter(count);

    /// <summary>Counts the general timer down, repeating <paramref name="program"/> while it is nonzero.</summary>
    private static AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue> RepeatWhileCounting(GunshipDustProgram program) => AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue>.RepeatWhileCounting(program);

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue> FallInto(GunshipDustProgram program) => AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue>.FallInto(program);

    /// <summary>Holds the last frame.</summary>
    private static AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue> Hold => AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue>.Hold;
}
