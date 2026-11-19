using SuperMetroid.Core.Enemies.MaridiaSnails.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>Cartridge values and helpers of <see cref="OumAnimations"/> that only verification reads.</summary>
internal static class OumAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params OumSegment[] programs) =>
        programs.SelectMany(program => OumAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => OumFrameToolingDefinitions.Spritemap(frame));
}
