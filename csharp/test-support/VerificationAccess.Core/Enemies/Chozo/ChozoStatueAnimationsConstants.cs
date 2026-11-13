using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Cartridge values and helpers of <see cref="ChozoStatueAnimations"/> that only verification reads.</summary>
internal static class ChozoStatueAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ChozoStatueSegment[] programs) =>
        programs.SelectMany(program => ChozoStatueAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, ChozoStatueCue>.Frame>()
            .Select(frame => ChozoStatueFrameToolingDefinitions.Spritemap(frame.Shown));
}
