using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Waver.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>Cartridge values and helpers of <see cref="WaverAnimations"/> that only verification reads.</summary>
internal static class WaverAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params WaverSegment[] programs) =>
        programs.SelectMany(program => WaverAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, WaverCue>.Frame>()
            .Select(frame => WaverFrameToolingDefinitions.Spritemap(frame.Shown));
}
