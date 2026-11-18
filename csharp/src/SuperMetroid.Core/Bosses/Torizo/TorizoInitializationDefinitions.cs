using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What Bomb and Golden Torizo start with: their starting places, first animations, processing and radii.</summary>
internal static class TorizoInitializationDefinitions
{
    /// <summary>
    /// Bomb Torizo's starting place, first body animation, processing and radii.
    /// The processing flags are added to the population's existing processing.
    /// </summary>
    internal static TorizoInitializationDefinition Bomb => new(new(219, 179), TorizoAnimation.BombTorizoDormant, EnemyFrameId.BombTorizoDormant, EnemyProcessing.OffScreen | EnemyProcessing.Animates, new(18, 48));

    /// <summary>
    /// Golden Torizo's starting place, first body animation, processing and radii.
    /// Its shorter Y radius is authored data.
    /// </summary>
    internal static TorizoInitializationDefinition Golden => new(new(424, 144), TorizoAnimation.GoldenTorizoDormant, EnemyFrameId.TorizoFacingViewer, EnemyProcessing.OffScreen | EnemyProcessing.Animates, new(18, 41));
}
