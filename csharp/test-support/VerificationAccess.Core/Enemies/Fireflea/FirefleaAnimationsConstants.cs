using SuperMetroid.Core.Enemies.Fireflea.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>Cartridge values and helpers of <see cref="FirefleaAnimations"/> that only verification reads.</summary>
internal static class FirefleaAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params FirefleaProgram[] programs) =>
        programs.SelectMany(program => FirefleaAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<FirefleaFrame, NoAnimationCue>.Frame>()
            .Select(frame => FirefleaFrameToolingDefinitions.Spritemap(frame.Shown));
}
