using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Stoke.Artwork;

namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>Cartridge values and helpers of <see cref="StokeAnimations"/> that only verification reads.</summary>
internal static class StokeAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params StokeProgram[] programs) =>
        programs.SelectMany(program => StokeAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<StokeFrame, StokeCue>.Frame>()
            .Select(frame => StokeFrameToolingDefinitions.Spritemap(frame.Shown));
}
