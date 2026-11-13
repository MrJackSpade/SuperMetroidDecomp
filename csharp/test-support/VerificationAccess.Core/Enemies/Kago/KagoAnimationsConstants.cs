using SuperMetroid.Core.Enemies.Kago.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Cartridge values and helpers of <see cref="KagoAnimations"/> that only verification reads.</summary>
internal static class KagoAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params KagoProgram[] programs) =>
        programs.SelectMany(program => KagoAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => KagoFrameToolingDefinitions.Spritemap(frame.Shown));
}
