using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Skultera.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>Cartridge values and helpers of <see cref="SkulteraAnimations"/> that only verification reads.</summary>
internal static class SkulteraAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params SkulteraProgram[] programs) =>
        programs.SelectMany(program => SkulteraAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, SkulteraCue>.Frame>()
            .Select(frame => SkulteraFrameToolingDefinitions.Spritemap(frame.Shown));
}
