using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.LavaJumpers.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>Cartridge values and helpers of <see cref="NorfairLavaJumperAnimations"/> that only verification reads.</summary>
internal static class NorfairLavaJumperAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params NorfairLavaJumperSegment[] programs) =>
        programs.SelectMany(program => NorfairLavaJumperAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NorfairLavaJumperCue>.Frame>()
            .Select(frame => NorfairLavaJumperFrameToolingDefinitions.Spritemap(frame.Shown));
}
