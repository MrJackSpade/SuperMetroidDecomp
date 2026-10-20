using P = SuperMetroid.Core.Enemies.FaceBlocks.BlueBrinstarFaceBlockProgram;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>
/// The Blue Brinstar face block's neutral and turning animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="BlueBrinstarFaceBlockFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class BlueBrinstarFaceBlockAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<BlueBrinstarFaceBlockProgram, BlueBrinstarFaceBlockFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<BlueBrinstarFaceBlockProgram, AnimationProgramStep<BlueBrinstarFaceBlockProgram, BlueBrinstarFaceBlockFrame, NoAnimationCue>[]>
    {
        [P.SamusLeft] =
        [
            Show(0, 48),
            Show(1, 16),
            Show(2, 16),
            Hold,
        ],
        [P.SamusRight] =
        [
            Show(0, 48),
            Show(3, 16),
            Show(4, 16),
            Hold,
        ],
        [P.Initial] =
        [
            Show(0, 1),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BlueBrinstarFaceBlockProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .OfType<AnimationProgramStep<P, BlueBrinstarFaceBlockFrame, NoAnimationCue>.ShowStep>()
            .Select(step => BlueBrinstarFaceBlockFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<BlueBrinstarFaceBlockProgram, BlueBrinstarFaceBlockFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<BlueBrinstarFaceBlockProgram, BlueBrinstarFaceBlockFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<BlueBrinstarFaceBlockProgram, BlueBrinstarFaceBlockFrame, NoAnimationCue> Hold => AnimationProgramStep<BlueBrinstarFaceBlockProgram, BlueBrinstarFaceBlockFrame, NoAnimationCue>.Hold;

}
