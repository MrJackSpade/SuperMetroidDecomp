using SuperMetroid.Core.Enemies.MamaTurtles.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>Development-tool members of <see cref="MamaTurtleAnimations"/>; never linked by player hosts.</summary>
internal static class MamaTurtleAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MamaTurtleProgram[] programs) =>
        programs.SelectMany(program => MamaTurtleAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => MamaTurtleFrameToolingDefinitions.Spritemap(frame));
}
