using P = SuperMetroid.Core.Enemies.Gunship.GunshipProgram;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship hull's and entrance pad's typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="GunshipFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class GunshipAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<GunshipProgram, GunshipFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<GunshipProgram, AnimationProgramStep<GunshipProgram, GunshipFrame, NoAnimationCue>[]>
    {
        // $A2:A5BE
        [P.EntrancePadOpening] =
        [
            Show(12, 40),
            Show(11, 8),
            Show(2, 8),
            Show(3, 8),
            Show(4, 24),
            Show(5, 8),
            Show(6, 7),
            Show(7, 6),
            Show(8, 5),
            Show(9, 4),
            FallInto(P.EntrancePadOpen),
        ],
        // $A2:A5E6
        [P.EntrancePadOpen] =
        [
            Show(10, 4),
            GoTo(P.EntrancePadOpen),
        ],
        // $A2:A5EE
        [P.EntrancePadClosing] =
        [
            Show(9, 4),
            Show(8, 5),
            Show(7, 6),
            Show(6, 7),
            Show(5, 8),
            Show(4, 24),
            Show(3, 8),
            Show(11, 8),
            FallInto(P.BottomEntrancePad),
        ],
        // $A2:A60E
        [P.BottomEntrancePad] =
        [
            Show(12, 8),
            GoTo(P.BottomEntrancePad),
        ],
        // $A2:A616
        [P.TopHull] =
        [
            Show(0, 1),
            Hold,
        ],
        // $A2:A61C
        [P.BottomHull] =
        [
            Show(1, 1),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params GunshipProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => GunshipFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<GunshipProgram, GunshipFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<GunshipProgram, GunshipFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<GunshipProgram, GunshipFrame, NoAnimationCue> GoTo(GunshipProgram program) => AnimationProgramStep<GunshipProgram, GunshipFrame, NoAnimationCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<GunshipProgram, GunshipFrame, NoAnimationCue> FallInto(GunshipProgram program) => AnimationProgramStep<GunshipProgram, GunshipFrame, NoAnimationCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<GunshipProgram, GunshipFrame, NoAnimationCue> Hold => AnimationProgramStep<GunshipProgram, GunshipFrame, NoAnimationCue>.Hold;

}
