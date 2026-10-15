using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Beetom.BeetomCueKind;
using P = SuperMetroid.Core.Enemies.Beetom.BeetomProgram;
using SuperMetroid.Core.Enemies.Beetom.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The Beetom's crawl, hop and drain animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="BeetomFrame"/> pose and each callback becomes a <see cref="BeetomCue"/> with its typed operands.
/// </summary>
internal static class BeetomAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<BeetomProgram, BeetomFrame, BeetomCue> Programs { get; } = new(
        new Dictionary<BeetomProgram, AnimationProgramStep<BeetomProgram, BeetomFrame, BeetomCue>[]>
    {
        // $A8:B696
        [P.CrawlingLeft] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            FallInto(P.CrawlingLeftLoop),
        ],
        // $A8:B698
        [P.CrawlingLeftLoop] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(1, 10),
            GoTo(P.CrawlingLeftLoop),
        ],
        // $A8:B6AC
        [P.HopLeft] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(3, 4),
            Show(4, 8),
            Show(3, 4),
            Show(0, 1),
            Hold,
        ],
        // $A8:B6CC
        [P.DrainingLeft] =
        [
            Show(5, 5),
            Show(6, 5),
            Show(7, 5),
            Show(6, 48),
            Cue(new(K.Nothing)),
            FallInto(P.DrainingLeftLoop),
        ],
        // $A8:B6DE
        [P.DrainingLeftLoop] =
        [
            Show(8, 5),
            Show(9, 5),
            Show(10, 5),
            Show(9, 5),
            GoTo(P.DrainingLeftLoop),
        ],
        // $A8:B6F2
        [P.CrawlingRight] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            FallInto(P.CrawlingRightLoop),
        ],
        // $A8:B6F4
        [P.CrawlingRightLoop] =
        [
            Show(11, 10),
            Show(12, 10),
            Show(13, 10),
            Show(12, 10),
            GoTo(P.CrawlingRightLoop),
        ],
        // $A8:B708
        [P.HopRight] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(14, 4),
            Show(15, 8),
            Show(14, 4),
            Show(11, 1),
            Hold,
        ],
        // $A8:B728
        [P.DrainingRight] =
        [
            Show(16, 5),
            Show(17, 5),
            Show(18, 5),
            Show(17, 48),
            Cue(new(K.Nothing)),
            FallInto(P.DrainingRightLoop),
        ],
        // $A8:B73A
        [P.DrainingRightLoop] =
        [
            Show(19, 5),
            Show(20, 5),
            Show(21, 5),
            Show(20, 5),
            GoTo(P.DrainingRightLoop),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<BeetomProgram, BeetomFrame, BeetomCue> Show(int pose, int duration) => AnimationProgramStep<BeetomProgram, BeetomFrame, BeetomCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<BeetomProgram, BeetomFrame, BeetomCue> Cue(BeetomCue cue) => AnimationProgramStep<BeetomProgram, BeetomFrame, BeetomCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<BeetomProgram, BeetomFrame, BeetomCue> GoTo(BeetomProgram program) => AnimationProgramStep<BeetomProgram, BeetomFrame, BeetomCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<BeetomProgram, BeetomFrame, BeetomCue> FallInto(BeetomProgram program) => AnimationProgramStep<BeetomProgram, BeetomFrame, BeetomCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<BeetomProgram, BeetomFrame, BeetomCue> Hold => AnimationProgramStep<BeetomProgram, BeetomFrame, BeetomCue>.Hold;

}
