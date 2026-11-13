using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Cartridge values and helpers of <see cref="NinjaSpacePirateAnimations"/> that only verification reads.</summary>
internal static class NinjaSpacePirateAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params NinjaSpacePirateSegment[] programs) =>
        programs.SelectMany(program => NinjaSpacePirateAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>.Frame>()
            .Select(frame => NinjaSpacePirateFrameToolingDefinitions.Spritemap(frame.Shown));
}
