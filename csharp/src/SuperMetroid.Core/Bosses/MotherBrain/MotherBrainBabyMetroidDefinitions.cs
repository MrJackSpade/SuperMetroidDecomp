using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawn and music identities for Mother Brain's cutscene Baby Metroid.</summary>
internal static class MotherBrainBabyMetroidDefinitions
{
    /// <summary>Track five follows the theme data upload.</summary>
    internal const int SamusThemeMusicTrack = 5;

    /// <summary>
    /// The single embedded population record. Baby init AI
    /// subsequently replaces the actor coordinates, but the record remains its physical spawn
    /// snapshot and supplies the property word. Its initialization parameter is zero: the
    /// original record carries the Baby's first animation script there, which the cutscene's own
    /// program replaces and nothing reads.
    /// </summary>
    internal static readonly RoomEnemyPopulationRecord SpawnPopulation =
        new(EnemyDefinitionId.BabyMetroidCutscene, new(384, 64),
            EnemyContact.None, EnemyProcessing.AnimationScript | EnemyProcessing.OffScreen, EnemyPresence.None,
            DrawsExtendedFrames: false, EnemySpawnSettings.None);
}
