using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Development-tool members of <see cref="DachoraAnimations"/>; never linked by player hosts.</summary>
internal static class DachoraAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params DachoraProgram[] programs) =>
        programs.SelectMany(program => DachoraAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => DachoraFrameToolingDefinitions.Spritemap(frame.Shown));
}
