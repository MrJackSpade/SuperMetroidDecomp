using P = SuperMetroid.Core.Enemies.FaceBlocks.BlueBrinstarFaceBlockProgram;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>Cartridge values and helpers of <see cref="BlueBrinstarFaceBlockAnimations"/> that only verification reads.</summary>
internal static class BlueBrinstarFaceBlockAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BlueBrinstarFaceBlockProgram[] programs) =>
        programs.SelectMany(program => BlueBrinstarFaceBlockAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, BlueBrinstarFaceBlockFrame, NoAnimationCue>.ShowStep>()
            .Select(step => BlueBrinstarFaceBlockFrameToolingDefinitions.Spritemap(step.Frame));
}
