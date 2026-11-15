using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Compiled parallel definitions used by the shared Bomb/Golden Torizo initializer.</summary>
internal static class TorizoInitializationDefinitions
{
    /// <summary>
    /// Bomb Torizo's starting place, first body animation, processing and radii, and its walking turns.
    /// The processing flags are added to the population's existing processing.
    /// </summary>
    internal static TorizoInitializationDefinition Bomb => new(new(219, 179), TorizoAnimation.BombTorizoDormant, EnemyProcessing.OffScreen | EnemyProcessing.Animates, new(18, 48),
        TorizoAnimation.TurningLeft, TorizoAnimation.TurningRight);

    /// <summary>
    /// Golden Torizo's starting place, first body animation, processing and radii, and its walking turns.
    /// Its shorter Y radius is authored data.
    /// </summary>
    internal static TorizoInitializationDefinition Golden => new(new(424, 144), TorizoAnimation.GoldenTorizoDormant, EnemyProcessing.OffScreen | EnemyProcessing.Animates, new(18, 41),
        TorizoAnimation.GoldenTurningLeft, TorizoAnimation.GoldenTurningRight);
}
