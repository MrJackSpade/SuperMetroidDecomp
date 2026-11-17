using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawn and music identities for Mother Brain's cutscene Baby Metroid.</summary>
internal static class MotherBrainBabyMetroidDefinitions
{
    /// <summary>The Samus theme, played once its data set has loaded.</summary>
    internal const MusicTrack SamusThemeMusicTrack = MusicTrack.SamusTheme;

    /// <summary>
    /// The Baby's placement, which supplies its properties. Its initialization replaces the placement's position, but
    /// the placement keeps it as where the Baby spawned. It carries no spawn settings: the cutscene starts the Baby's
    /// own animation.
    /// </summary>
    internal static readonly RoomEnemyPlacement SpawnPopulation =
        new(EnemyDefinitionId.BabyMetroidCutscene, new(384, 64),
            EnemyContact.None, EnemyProcessing.Animates | EnemyProcessing.OffScreen, EnemyPresence.None,
            CollidesByFrameHitboxes: false, EnemySpawnSettings.None);
}
