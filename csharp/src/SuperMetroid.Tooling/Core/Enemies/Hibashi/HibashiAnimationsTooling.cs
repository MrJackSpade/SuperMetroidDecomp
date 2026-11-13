using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Hibashi.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>Development-tool members of <see cref="HibashiAnimations"/>; never linked by player hosts.</summary>
internal static class HibashiAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params HibashiSegment[] programs) =>
        programs.SelectMany(program => HibashiAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, HibashiCue>.Frame>()
            .Select(frame => HibashiFrameToolingDefinitions.Spritemap(frame.Shown));
}
