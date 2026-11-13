using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Shaktool.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Cartridge values and helpers of <see cref="ShaktoolAnimations"/> that only verification reads.</summary>
internal static class ShaktoolAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ShaktoolProgram[] programs) =>
        programs.SelectMany(program => ShaktoolAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, ShaktoolCue>.Frame>()
            .Select(frame => ShaktoolFrameToolingDefinitions.Spritemap(frame.Shown));
}
