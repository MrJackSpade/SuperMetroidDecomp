using SuperMetroid.Core.Enemies.Boulder.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>Cartridge values and helpers of <see cref="BoulderAnimations"/> that only verification reads.</summary>
internal static class BoulderAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BoulderProgram[] programs) =>
        programs.SelectMany(program => BoulderAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<BoulderFrame, NoAnimationCue>.Frame>()
            .Select(frame => BoulderFrameToolingDefinitions.Spritemap(frame.Shown));
}
