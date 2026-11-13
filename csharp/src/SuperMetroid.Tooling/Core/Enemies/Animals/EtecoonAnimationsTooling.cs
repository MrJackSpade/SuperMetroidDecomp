using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Development-tool members of <see cref="EtecoonAnimations"/>; never linked by player hosts.</summary>
internal static class EtecoonAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params EtecoonSegment[] programs) =>
        programs.SelectMany(program => EtecoonAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => EtecoonFrameToolingDefinitions.Spritemap(frame.Shown));
}
