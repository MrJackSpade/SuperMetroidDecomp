using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Evir.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>Cartridge values and helpers of <see cref="EvirAnimations"/> that only verification reads.</summary>
internal static class EvirAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params EvirProgram[] programs) =>
        programs.SelectMany(program => EvirAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, EvirCue>.Frame>()
            .Select(frame => EvirFrameToolingDefinitions.Spritemap(frame.Shown));
}
