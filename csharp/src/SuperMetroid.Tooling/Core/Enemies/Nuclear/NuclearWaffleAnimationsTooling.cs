using SuperMetroid.Core.Enemies.Nuclear.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Development-tool members of <see cref="NuclearWaffleAnimations"/>; never linked by player hosts.</summary>
internal static class NuclearWaffleAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params NuclearWaffleSegment[] programs) =>
        programs.SelectMany(program => NuclearWaffleAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => NuclearWaffleFrameToolingDefinitions.Spritemap(frame.Shown));
}
