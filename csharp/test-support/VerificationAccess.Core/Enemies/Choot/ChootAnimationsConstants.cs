using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Choot.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Cartridge values and helpers of <see cref="ChootAnimations"/> that only verification reads.</summary>
internal static class ChootAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ChootProgram[] programs) =>
        programs.SelectMany(program => ChootAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, ChootCue>.Frame>()
            .Select(frame => ChootFrameToolingDefinitions.Spritemap(frame.Shown));
}
