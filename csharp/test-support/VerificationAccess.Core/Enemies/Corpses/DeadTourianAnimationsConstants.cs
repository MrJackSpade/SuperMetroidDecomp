using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Cartridge values and helpers of <see cref="DeadTourianAnimations"/> that only verification reads.</summary>
internal static class DeadTourianAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params DeadTourianSegment[] programs) =>
        programs.SelectMany(program => DeadTourianAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, DeadTourianCue>.Frame>()
            .Select(frame => DeadTourianFrameToolingDefinitions.Spritemap(frame.Shown));
}
