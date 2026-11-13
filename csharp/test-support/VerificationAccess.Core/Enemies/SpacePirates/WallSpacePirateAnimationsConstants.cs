using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Cartridge values and helpers of <see cref="WallSpacePirateAnimations"/> that only verification reads.</summary>
internal static class WallSpacePirateAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params WallSpacePirateSegment[] programs) =>
        programs.SelectMany(program => WallSpacePirateAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, WallSpacePirateCue>.Frame>()
            .Select(frame => WallSpacePirateFrameToolingDefinitions.Spritemap(frame.Shown));
}
