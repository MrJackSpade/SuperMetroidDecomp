using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Cartridge values and helpers of <see cref="NorfairRioAnimations"/> that only verification reads.</summary>
internal static class NorfairRioAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params NorfairRioProgram[] programs) =>
        programs.SelectMany(program => NorfairRioAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NorfairRioCue>.Frame>()
            .Select(frame => NorfairRioFrameToolingDefinitions.Spritemap(frame.Shown));
}
