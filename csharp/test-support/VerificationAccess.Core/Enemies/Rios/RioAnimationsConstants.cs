using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Cartridge values and helpers of <see cref="RioAnimations"/> that only verification reads.</summary>
internal static class RioAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params RioProgram[] programs) =>
        programs.SelectMany(program => RioAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, RioCue>.Frame>()
            .Select(frame => RioFrameToolingDefinitions.Spritemap(frame.Shown));
}
