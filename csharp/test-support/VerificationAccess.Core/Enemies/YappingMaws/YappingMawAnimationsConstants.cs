using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.YappingMaws.YappingMawProgram;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Cartridge values and helpers of <see cref="YappingMawAnimations"/> that only verification reads.</summary>
internal static class YappingMawAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params YappingMawProgram[] programs) =>
        programs.SelectMany(program => YappingMawAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<YappingMawFrame, YappingMawCue>.Frame>()
            .Select(frame => YappingMawFrameToolingDefinitions.Spritemap(frame.Shown));
}
