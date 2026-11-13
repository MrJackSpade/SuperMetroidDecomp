using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>Cartridge values and helpers of <see cref="FuneNamiheAnimations"/> that only verification reads.</summary>
internal static class FuneNamiheAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params FuneNamiheSegment[] programs) =>
        programs.SelectMany(program => FuneNamiheAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, FuneNamiheCue>.Frame>()
            .Select(frame => FuneNamiheFrameToolingDefinitions.Spritemap(frame.Shown));
}
