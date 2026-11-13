using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Development-tool members of <see cref="EscapeDachoraAnimations"/>; never linked by player hosts.</summary>
internal static class EscapeDachoraAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params EscapeDachoraProgram[] programs) =>
        programs.SelectMany(program => EscapeDachoraAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, EscapeDachoraCue>.Frame>()
            .Select(frame => EscapeDachoraFrameToolingDefinitions.Spritemap(frame.Shown));
}
