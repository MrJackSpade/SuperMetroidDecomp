using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Compiled parallel records used by the shared Bomb/Golden Torizo initializer.</summary>
internal static class TorizoInitializationDefinitions
{
    /// <summary>
    /// The Torizo initializer's positions, animations, processing and radii.
    /// First entries select Bomb Torizo, including its first initial body animation.
    /// The processing flags are added to the population's existing processing.
    /// </summary>
    internal static TorizoInitializationDefinition Bomb => new(new(219, 179), TorizoAnimation.BombTorizoDormant, EnemyProcessing.OffScreen | EnemyProcessing.Animates, new(18, 48));

    /// <summary>
    /// The second initialization entries select Golden Torizo, including
    /// its first initial body animation. Its shorter Y radius is authored data.
    /// </summary>
    internal static TorizoInitializationDefinition Golden => new(new(424, 144), TorizoAnimation.GoldenTorizoDormant, EnemyProcessing.OffScreen | EnemyProcessing.Animates, new(18, 41));
}
