namespace SuperMetroid.Core.Game;

/// <summary>Spawn and music identities for Mother Brain's cutscene Baby Metroid.</summary>
internal static class MotherBrainBabyMetroidDefinitions
{
    /// <summary>$A9:CC65, Function_BabyMetroidCutscene_PlaySamusTheme: data command $FF48 loads the Theme of Samus.</summary>
    internal const byte SamusThemeMusicData = 0x48;

    /// <summary>$A9:CC6C, Function_BabyMetroidCutscene_PlaySamusTheme: track $0005 follows the theme data upload.</summary>
    internal const byte SamusThemeMusicTrack = 5;

    /// <summary>
    /// The single embedded population record at <c>$A9:BE28-$A9:BE37</c>. Baby init AI
    /// subsequently replaces the actor coordinates, but the complete record remains its
    /// physical spawn snapshot and supplies the native property word.
    /// </summary>
    internal static readonly RoomEnemyPopulationRecord SpawnPopulation =
        new(EnemyDefinitionId.BabyMetroidCutscene, 0x0180, 0x0040, 0xcfa2, 0x2800, 0, 0, 0);
}
