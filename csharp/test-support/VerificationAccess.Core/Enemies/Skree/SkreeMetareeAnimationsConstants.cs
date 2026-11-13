using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Skree.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Cartridge values and helpers of <see cref="SkreeMetareeAnimations"/> that only verification reads.</summary>
internal static class SkreeMetareeAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params SkreeMetareeSegment[] programs) =>
        programs.SelectMany(program => SkreeMetareeAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, SkreeMetareeCue>.Frame>()
            .Select(frame => SkreeMetareeFrameToolingDefinitions.Spritemap(frame.Shown));
}
