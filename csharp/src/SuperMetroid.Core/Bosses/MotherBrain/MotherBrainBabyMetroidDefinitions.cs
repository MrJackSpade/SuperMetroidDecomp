using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawn and music identities for Mother Brain's cutscene Baby Metroid.</summary>
internal static class MotherBrainBabyMetroidDefinitions
{
    /// <summary>The Samus theme, played once its data set has loaded.</summary>
    internal const MusicTrack SamusThemeMusicTrack = MusicTrack.SamusTheme;

    /// <summary>
    /// The single embedded population record. Baby init AI
    /// subsequently replaces the actor coordinates, but the record remains its physical spawn
    /// snapshot and supplies its properties. Its initialization parameter is zero: the
    /// game's record carries the Baby's first animation there, which the cutscene's own
    /// animation replaces and nothing reads.
    /// </summary>
    internal static readonly RoomEnemyPopulationRecord SpawnPopulation =
        new(EnemyDefinitionId.BabyMetroidCutscene, new(384, 64),
            EnemyContact.None, EnemyProcessing.Animates | EnemyProcessing.OffScreen, EnemyPresence.None,
            CollidesByFrameHitboxes: false, EnemySpawnSettings.None);
}
