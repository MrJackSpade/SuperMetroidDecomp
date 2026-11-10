using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>Cartridge values and helpers of <see cref="BlueBrinstarFaceBlockAnimations"/> that only verification reads.</summary>
internal static class BlueBrinstarFaceBlockAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BlueBrinstarFaceBlockProgram[] programs) =>
        programs.SelectMany(program => BlueBrinstarFaceBlockAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<BlueBrinstarFaceBlockFrame, NoAnimationCue>.Frame>()
            .Select(frame => BlueBrinstarFaceBlockFrameToolingDefinitions.Spritemap(frame.Shown));
}
