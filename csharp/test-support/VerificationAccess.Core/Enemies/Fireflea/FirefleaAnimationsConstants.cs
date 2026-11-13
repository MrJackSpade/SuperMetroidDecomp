using SuperMetroid.Core.Enemies.Fireflea.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>Cartridge values and helpers of <see cref="FirefleaAnimations"/> that only verification reads.</summary>
internal static class FirefleaAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params FirefleaProgram[] programs) =>
        programs.SelectMany(program => FirefleaAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => FirefleaFrameToolingDefinitions.Spritemap(frame.Shown));
}
