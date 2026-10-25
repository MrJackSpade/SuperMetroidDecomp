using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawn and music identities for Mother Brain's cutscene Baby Metroid.</summary>
internal static class MotherBrainBabyMetroidDefinitions
{
    /// <summary>$A9:CC6C, Function_BabyMetroidCutscene_PlaySamusTheme: track $0005 follows the theme data upload.</summary>
    internal const byte SamusThemeMusicTrack = 5;

    /// <summary>
    /// The single embedded population record at <c>$A9:BE28-$A9:BE37</c>. Baby init AI
    /// subsequently replaces the actor coordinates, but the record remains its physical spawn
    /// snapshot and supplies the native property word. Its initialization parameter is zero: the
    /// cartridge's record carries the Baby's first instruction list there, which the cutscene's own
    /// program replaces and nothing reads.
    /// </summary>
    internal static readonly RoomEnemyPopulationRecord SpawnPopulation =
        new(EnemyDefinitionId.BabyMetroidCutscene, new(0x0180, 0x0040), 0,
            new EnemyPropertyWord(EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen), new EnemyExtraPropertyWord(EnemyExtraProperties.None), 0, 0);
}
