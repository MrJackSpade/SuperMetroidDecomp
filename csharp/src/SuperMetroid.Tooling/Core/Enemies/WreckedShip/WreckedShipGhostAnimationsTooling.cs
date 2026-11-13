using SuperMetroid.Core.Enemies.WreckedShip.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Development-tool members of <see cref="WreckedShipGhostAnimations"/>; never linked by player hosts.</summary>
internal static class WreckedShipGhostAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params WreckedShipGhostSegment[] programs) =>
        programs.SelectMany(program => WreckedShipGhostAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => WreckedShipGhostFrameToolingDefinitions.Spritemap(frame.Shown));
}
