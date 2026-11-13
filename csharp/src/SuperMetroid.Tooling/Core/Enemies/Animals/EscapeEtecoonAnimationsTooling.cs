using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Development-tool members of <see cref="EscapeEtecoonAnimations"/>; never linked by player hosts.</summary>
internal static class EscapeEtecoonAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params EscapeEtecoonSegment[] programs) =>
        programs.SelectMany(program => EscapeEtecoonAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, EscapeEtecoonCue>.Frame>()
            .Select(frame => EscapeEtecoonFrameToolingDefinitions.Spritemap(frame.Shown));
}
