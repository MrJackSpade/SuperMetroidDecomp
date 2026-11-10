using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>Cartridge values and helpers of <see cref="AlcoonAnimations"/> that only verification reads.</summary>
internal static class AlcoonAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params AlcoonProgram[] programs) =>
        programs.SelectMany(program => AlcoonAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<AlcoonFrame, AlcoonCue>.Frame>()
            .Select(frame => AlcoonFrameToolingDefinitions.Spritemap(frame.Shown));
}
